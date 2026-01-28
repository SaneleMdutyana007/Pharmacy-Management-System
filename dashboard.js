// dashboard.js
console.log('Dashboard script loaded');

// Store notification data for modal
let notificationData = [];
let currentNotificationId = null;

// URLs for navigation (these will be set from the View)
let appUrls = {};

document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded, initializing dashboard...');

    // Initialize URLs from data attributes
    initializeUrls();

    // Initialize notifications
    loadNotifications();

    // Auto-refresh notifications every 30 seconds
    setInterval(loadNotifications, 5000);

    // Check for new notifications every minute
    setInterval(loadNotificationCount, 5000);

    // Setup modal event listeners
    setupModalEvents();
});

function initializeUrls() {
    // Get URLs from data attributes on the body or a hidden element
    const urlContainer = document.getElementById('urls-data') || document.body;
    appUrls = {
        medicationDetails: urlContainer.getAttribute('data-url-medication-details') || '/Medications/Details',
        doctorDetails: urlContainer.getAttribute('data-url-doctor-details') || '/Doctors/Details',
        supplierDetails: urlContainer.getAttribute('data-url-supplier-details') || '/Suppliers/Details',
        orderDetails: urlContainer.getAttribute('data-url-order-details') || '/Orders/Details',
        getNotifications: urlContainer.getAttribute('data-url-get-notifications') || '/Dashboard/GetRecentNotifications',
        getNotificationCount: urlContainer.getAttribute('data-url-get-notification-count') || '/Dashboard/GetNotificationCount',
        markAsRead: urlContainer.getAttribute('data-url-mark-as-read') || '/Dashboard/MarkNotificationAsRead'
    };
}

function setupModalEvents() {
    const modal = document.getElementById('notificationModal');
    if (modal) {
        modal.addEventListener('hidden.bs.modal', function () {
            currentNotificationId = null;
        });
    }
}

function loadNotifications() {
    console.log('Loading notifications...');

    fetch(appUrls.getNotifications)
        .then(response => {
            console.log('Notifications response status:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Notifications data received:', data);
            notificationData = data;
            updateNotificationsUI(data);
        })
        .catch(error => {
            console.error('Error loading notifications:', error);
            showNotificationError();
        });
}

function loadNotificationCount() {
    console.log('Loading notification count...');

    fetch(appUrls.getNotificationCount)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Notification count:', data.count);
            const notificationCount = document.getElementById('notificationCount');
            if (notificationCount && data.count !== undefined) {
                notificationCount.textContent = data.count;
            }
        })
        .catch(error => {
            console.error('Error loading notification count:', error);
            const notificationCount = document.getElementById('notificationCount');
            if (notificationCount) {
                notificationCount.textContent = '0';
            }
        });
}

function updateNotificationsUI(notifications) {
    const notificationList = document.getElementById('notificationList');
    const notificationCount = document.getElementById('notificationCount');

    if (!notificationList) {
        console.error('Notification list element not found');
        return;
    }

    console.log('Updating notifications UI with:', notifications);

    // Check if notifications is an array
    if (!Array.isArray(notifications)) {
        console.error('Notifications is not an array:', notifications);
        showNotificationError();
        return;
    }

    if (notifications.length === 0) {
        notificationList.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-bell-slash fa-3x mb-3 text-muted"></i>
                <h4>No Notifications</h4>
                <p class="text-muted">Updates will appear here when you have new activity.</p>
            </div>
        `;
        if (notificationCount) {
            notificationCount.textContent = '0';
        }
        return;
    }

    if (notificationCount) {
        notificationCount.textContent = notifications.length;
    }

    let notificationsHTML = '';

    notifications.forEach(notification => {
        const iconClass = getNotificationIcon(notification.type);
        const timeAgo = getTimeAgo(notification.timestamp);
        const truncatedMessage = truncateMessage(notification.message, 80);

        notificationsHTML += `
            <div class="notification-item" data-notification-id="${notification.id}" onclick="showNotificationModal(${notification.id})">
                <div class="notification-icon ${iconClass.iconClass}">
                    <i class="${iconClass.icon}"></i>
                </div>
                <div class="notification-content">
                    <p class="notification-message">${escapeHtml(truncatedMessage)}</p>
                    <p class="notification-time">${timeAgo}</p>
                </div>
                <button class="notification-mark-read" onclick="event.stopPropagation(); markAsRead(${notification.id})" title="Remove notification">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
    });

    notificationList.innerHTML = notificationsHTML;
    console.log('Notifications UI updated successfully');
}

function showNotificationModal(notificationId) {
    const notification = notificationData.find(n => n.id === notificationId);
    if (!notification) return;

    currentNotificationId = notificationId;

    const iconClass = getNotificationIcon(notification.type);
    const fullTime = new Date(notification.timestamp).toLocaleString();
    const modalTitle = getNotificationTitle(notification.type);

    // Update modal content
    document.getElementById('modalIcon').className = `modal-icon ${iconClass.iconClass}`;
    document.getElementById('modalIcon').innerHTML = `<i class="${iconClass.icon}"></i>`;
    document.getElementById('modalTitle').textContent = modalTitle;
    document.getElementById('modalTime').textContent = fullTime;
    document.getElementById('modalMessage').textContent = notification.message;

    // Setup view details button
    const viewDetailsBtn = document.getElementById('viewDetailsBtn');
    if (notification.entityType && notification.entityId) {
        viewDetailsBtn.style.display = 'inline-block';
        viewDetailsBtn.onclick = function() {
            handleNotificationAction(notification.id, notification.entityType, notification.entityId);
        };
    } else {
        viewDetailsBtn.style.display = 'none';
    }

    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('notificationModal'));
    modal.show();
}

function markAsReadFromModal() {
    if (currentNotificationId) {
        markAsRead(currentNotificationId);
        const modal = bootstrap.Modal.getInstance(document.getElementById('notificationModal'));
        modal.hide();
    }
}

function handleNotificationAction(notificationId, entityType, entityId) {
    console.log('Handling notification action:', entityType, entityId);
    
    // Mark as read first
    markAsRead(notificationId);
    
    // Close modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('notificationModal'));
    modal.hide();
    
    // Navigate based on entity type using proper URLs
    let redirectUrl = '';
    switch (entityType?.toLowerCase()) {
        case 'medication':
            redirectUrl = `${appUrls.medicationDetails}/${entityId}`;
            break;
        case 'doctor':
            redirectUrl = `${appUrls.doctorDetails}/${entityId}`;
            break;
        case 'supplier':
            redirectUrl = `${appUrls.supplierDetails}/${entityId}`;
            break;
        case 'order':
            redirectUrl = `${appUrls.orderDetails}/${entityId}`;
            break;
        default:
            console.log('No specific action for entity type:', entityType);
            return;
    }
    
    if (redirectUrl) {
        window.location.href = redirectUrl;
    }
}

function truncateMessage(message, maxLength) {
    if (!message) return '';
    if (message.length <= maxLength) return message;
    return message.substring(0, maxLength) + '...';
}

function getNotificationTitle(type) {
    const titles = {
        'stock_low': 'Low Stock Alert',
        'stock_critical': 'Critical Stock Alert',
        'doctor_added': 'New Doctor Added',
        'supplier_added': 'New Supplier Added',
        'medicine_added': 'New Medication Added',
        'medicine_removed': 'Medication Removed',
        'order_completed': 'Order Completed',
        'order_pending': 'Order Pending',
        'order_placed': 'New Order Placed'
    };
    
    return titles[type] || 'Notification';
}

function getNotificationIcon(type) {
    const icons = {
        'stock_low': { icon: 'fas fa-exclamation-triangle', iconClass: 'icon-alert' },
        'stock_critical': { icon: 'fas fa-exclamation-circle', iconClass: 'icon-alert' },
        'doctor_added': { icon: 'fas fa-user-md', iconClass: 'icon-doctor' },
        'supplier_added': { icon: 'fas fa-truck', iconClass: 'icon-supplier' },
        'medicine_added': { icon: 'fas fa-pills', iconClass: 'icon-stock' },
        'medicine_removed': { icon: 'fas fa-trash', iconClass: 'icon-alert' },
        'order_completed': { icon: 'fas fa-check-circle', iconClass: 'icon-success' },
        'order_pending': { icon: 'fas fa-clock', iconClass: 'icon-warning' },
        'order_placed': { icon: 'fas fa-shopping-cart', iconClass: 'icon-info' }
    };

    return icons[type] || { icon: 'fas fa-bell', iconClass: 'icon-default' };
}

function getTimeAgo(timestamp) {
    try {
        const now = new Date();
        const time = new Date(timestamp);
        const diffInMinutes = Math.floor((now - time) / (1000 * 60));
        const diffInHours = Math.floor(diffInMinutes / 60);
        const diffInDays = Math.floor(diffInHours / 24);

        if (diffInMinutes < 1) return 'Just now';
        if (diffInMinutes < 60) return `${diffInMinutes}m ago`;
        if (diffInHours < 24) return `${diffInHours}h ago`;
        if (diffInDays === 1) return '1 day ago';
        return `${diffInDays}d ago`;
    } catch (error) {
        console.error('Error calculating time ago:', error);
        return 'Recently';
    }
}

function markAsRead(notificationId) {
    console.log('Marking notification as read:', notificationId);

    // Get anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    fetch(appUrls.markAsRead, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token || '',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify({ id: notificationId })
    })
        .then(response => response.json())
        .then(data => {
            console.log('Mark as read response:', data);
            if (data.success) {
                // Remove from UI with animation
                const notificationElement = document.querySelector(`[data-notification-id="${notificationId}"]`);
                if (notificationElement) {
                    notificationElement.style.opacity = '0';
                    notificationElement.style.transform = 'translateX(-20px)';
                    setTimeout(() => {
                        notificationElement.remove();
                        // Reload if this was the last notification
                        const remainingNotifications = document.querySelectorAll('.notification-item').length;
                        if (remainingNotifications === 0) {
                            loadNotifications();
                        }
                    }, 300);
                }
                loadNotificationCount();
            }
        })
        .catch(error => console.error('Error marking notification as read:', error));
}

function showNotificationError() {
    const notificationList = document.getElementById('notificationList');
    if (notificationList) {
        notificationList.innerHTML = `
            <div class="text-center text-danger py-4">
                <i class="fas fa-exclamation-triangle fa-2x mb-2"></i>
                <p>Error loading notifications</p>
                <p class="small text-muted">Check browser console for details</p>
                <button class="btn btn-sm btn-outline-primary mt-2" onclick="loadNotifications()">
                    <i class="fas fa-redo"></i> Retry
                </button>
            </div>
        `;
    }
}

function escapeHtml(unsafe) {
    if (unsafe === null || unsafe === undefined) return '';
    return unsafe
        .toString()
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

// Global error handler to catch any unhandled errors
window.addEventListener('error', function (e) {
    console.error('Global error caught:', e.error);
    console.error('Error details:', e.message, e.filename, e.lineno);
});