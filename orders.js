
    // Global variables
    let medications = [];

    // ===== INITIALIZATION =====
    document.addEventListener('DOMContentLoaded', function() {
        initializeTabs();
        loadMedications();
        setupEventListeners();
    });

    function initializeTabs() {
        document.querySelectorAll('.tab-btn').forEach(btn => {
            btn.addEventListener('click', function() {
                const targetTab = this.getAttribute('data-tab');

                // Update active tab button
                document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
                this.classList.add('active');

                // Show target tab content
                document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
                document.getElementById(targetTab).classList.add('active');

                // Load data for the tab
                if (targetTab === 'pending-orders') {
                    loadPendingOrders();
                } else if (targetTab === 'completed-orders') {
                    loadCompletedOrders();
                }
            });
        });
    }

    function setupEventListeners() {
        document.getElementById('clear-order').addEventListener('click', function() {
            document.querySelectorAll('.med-checkbox').forEach(cb => cb.checked = false);
            document.querySelectorAll('.order-qty').forEach(input => input.disabled = true);
            document.getElementById('select-all').checked = false;
            updateOrderSummary();
        });

        document.getElementById('place-order').addEventListener('click', placeOrder);
        document.getElementById('stock-filter').addEventListener('change', renderMedications);
        document.getElementById('order-supplier').addEventListener('change', function() {
            renderMedications();
            if (this.value && this.value !== 'all') hideSupplierValidationWarning();
        });
    }

    // ===== MEDICATION MANAGEMENT =====
    async function loadMedications() {
        try {
            const response = await fetch('@Url.Action("GetMedications", "Orders")');
            const data = await response.json();

            if (data.success) {
                medications = data.medications;
                renderMedications();
            } else {
                showNotification('Error loading medications: ' + data.error, 'error');
            }
        } catch (error) {
            showNotification('Error loading medications. Please try again.', 'error');
        }
    }

    function renderMedications() {
        const tbody = document.getElementById('medication-table-body');
        tbody.innerHTML = '';

        const stockFilter = document.getElementById('stock-filter').value;
        const supplierFilter = document.getElementById('order-supplier').value;

        const filteredMedications = medications.filter(med => {
            const status = getStockStatus(med);
            if (stockFilter !== 'all' && status !== stockFilter) return false;
            if (supplierFilter !== 'all' && med.supplier !== supplierFilter) return false;
            return true;
        });

        if (filteredMedications.length === 0) {
            tbody.innerHTML = `<tr><td colspan="8" class="no-data">No medications found matching the current filters.</td></tr>`;
            return;
        }

        filteredMedications.forEach(med => {
            const status = getStockStatus(med);
            const rowClass = status === 'critical' ? 'critical-stock' : status === 'low' ? 'low-stock' : '';
            const suggestedQty = Math.max(med.reorderLevel * 2 - med.currentStock, 10);

            const row = document.createElement('tr');
            row.className = rowClass;
            row.innerHTML = `
                <td><input type="checkbox" class="med-checkbox" data-id="${med.id}" data-price="${med.unitPrice}"></td>
                <td>${med.name}</td>
                <td>${med.supplier || 'N/A'}</td>
                <td>${med.currentStock}</td>
                <td>${med.reorderLevel}</td>
                <td><input type="number" class="order-qty" value="${suggestedQty}" min="1" disabled></td>
                <td>R ${med.unitPrice.toFixed(2)}</td>
                <td><span class="stock-status status-${status}">${status.charAt(0).toUpperCase() + status.slice(1)}</span></td>
            `;
            tbody.appendChild(row);
        });

        // Add event listeners to dynamic elements
        document.querySelectorAll('.med-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', function() {
                const qtyInput = this.closest('tr').querySelector('.order-qty');
                qtyInput.disabled = !this.checked;
                updateOrderSummary();
            });
        });

        document.querySelectorAll('.order-qty').forEach(input => {
            input.addEventListener('input', updateOrderSummary);
        });

        document.getElementById('select-all').addEventListener('change', function() {
            const checkboxes = document.querySelectorAll('.med-checkbox');
            const qtyInputs = document.querySelectorAll('.order-qty');
            checkboxes.forEach(cb => cb.checked = this.checked);
            qtyInputs.forEach(input => input.disabled = !this.checked);
            updateOrderSummary();
        });
    }

    function getStockStatus(medication) {
        if (medication.currentStock <= medication.reorderLevel) return 'critical';
        if (medication.currentStock <= medication.reorderLevel + 10) return 'low';
        return 'adequate';
    }

    function updateOrderSummary() {
        let totalItems = 0, totalQty = 0, totalCost = 0;

        document.querySelectorAll('.med-checkbox:checked').forEach(checkbox => {
            const qtyInput = checkbox.closest('tr').querySelector('.order-qty');
            const qty = parseInt(qtyInput.value) || 0;
            const price = parseFloat(checkbox.getAttribute('data-price'));
            totalItems++;
            totalQty += qty;
            totalCost += qty * price;
        });

        document.getElementById('total-items').textContent = totalItems;
        document.getElementById('total-qty').textContent = totalQty;
        document.getElementById('total-cost').textContent = `R ${totalCost.toFixed(2)}`;
    }

    // ===== ORDER MANAGEMENT =====
    async function placeOrder() {
        const selectedSupplier = document.getElementById('order-supplier').value;
        const selectedItems = getSelectedItems();

        if (!selectedSupplier || selectedSupplier === 'all') {
            showSupplierValidationWarning();
            showNotification('Please select a specific supplier.', 'error');
            return;
        }

        if (selectedItems.length === 0) {
            showNotification('Please select at least one medication.', 'error');
            return;
        }

        hideSupplierValidationWarning();

        const placeOrderButton = document.getElementById('place-order');
        const originalText = placeOrderButton.innerHTML;
        placeOrderButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Placing Order...';
        placeOrderButton.disabled = true;

        try {
            const totalQuantity = selectedItems.reduce((sum, item) => sum + item.quantity, 0);
            const totalCost = selectedItems.reduce((sum, item) => sum + (item.quantity * item.unitPrice), 0);

            const orderData = {
                supplier: selectedSupplier,
                medications: selectedItems,
                totalQuantity: totalQuantity,
                totalCost: totalCost
            };

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const response = await fetch('@Url.Action("PlaceOrder", "Orders")', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token || ''
                },
                body: JSON.stringify(orderData)
            });

            const data = await response.json();

            if (data.success) {
                showNotification('Order placed successfully!', 'success');
                document.getElementById('clear-order').click();
                setTimeout(() => {
                    document.querySelector('[data-tab="pending-orders"]').click();
                }, 1500);
            } else {
                showNotification('Error: ' + data.error, 'error');
            }
        } catch (error) {
            showNotification('Network error. Please try again.', 'error');
        } finally {
            placeOrderButton.innerHTML = originalText;
            placeOrderButton.disabled = false;
        }
    }

    async function loadPendingOrders() {
        try {
            showLoading('pending-loading', true);
            const response = await fetch('@Url.Action("GetPendingOrders", "Orders")');
            const data = await response.json();
            const tbody = document.getElementById('pending-orders-body');
            tbody.innerHTML = '';

            if (data.success && data.orders && data.orders.length > 0) {
                data.orders.forEach(order => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td>ORD-${order.orderId.toString().padStart(4, '0')}</td>
                        <td>${order.supplier}</td>
                        <td>${formatDate(order.orderDate)}</td>
                        <td>${order.items}</td>
                        <td>R ${order.total?.toFixed(2) || '0.00'}</td>
                        <td><span class="stock-status status-pending">${order.status}</span></td>
                        <td>
                            <div class="action-buttons">
                                <button class="btn btn-secondary view-order-btn" data-order-id="${order.orderId}">
                                    <i class="fas fa-eye"></i> View Details
                                </button>
                            </div>
                        </td>
                    `;
                    tbody.appendChild(row);
                });

                // Add event listeners to view buttons
                document.querySelectorAll('.view-order-btn').forEach(btn => {
                    btn.addEventListener('click', function() {
                        const orderId = this.getAttribute('data-order-id');
                        viewOrderDetails(orderId);
                    });
                });
            } else {
                tbody.innerHTML = `
                    <tr>
                        <td colspan="7" class="no-data">
                            <i class="fas fa-inbox" style="font-size: 24px; margin-bottom: 10px; display: block;"></i>
                            No pending orders found.
                        </td>
                    </tr>
                `;
            }
        } catch (error) {
            console.error('Error loading pending orders:', error);
            showNotification('Error loading pending orders.', 'error');
        } finally {
            showLoading('pending-loading', false);
        }
    }

    async function loadCompletedOrders() {
        try {
            showLoading('completed-loading', true);
            const response = await fetch('@Url.Action("GetCompletedOrders", "Orders")');
            const data = await response.json();
            const tbody = document.getElementById('completed-orders-body');
            tbody.innerHTML = '';

            if (data.success && data.orders && data.orders.length > 0) {
                data.orders.forEach(order => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td>ORD-${order.orderId.toString().padStart(4, '0')}</td>
                        <td>${order.supplier}</td>
                        <td>${formatDate(order.orderDate)}</td>
                        <td>${order.receivedDate && order.receivedDate !== 'Not received' ? formatDate(order.receivedDate) : 'Not received'}</td>
                        <td>${order.items}</td>
                        <td>R ${order.total?.toFixed(2) || '0.00'}</td>
                        <td><span class="stock-status status-completed">${order.status}</span></td>
                    `;
                    tbody.appendChild(row);
                });
            } else {
                tbody.innerHTML = `
                    <tr>
                        <td colspan="7" class="no-data">
                            <i class="fas fa-check-circle" style="font-size: 24px; margin-bottom: 10px; display: block;"></i>
                            No completed orders found.
                        </td>
                    </tr>
                `;
            }
        } catch (error) {
            console.error('Error loading completed orders:', error);
            showNotification('Error loading completed orders.', 'error');
        } finally {
            showLoading('completed-loading', false);
        }
    }

    async function deleteOrder(orderId) {
        if (!confirm('Are you sure you want to delete this order? This action cannot be undone.')) {
            return;
        }

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const response = await fetch('@Url.Action("DeleteOrder", "Orders")', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token || ''
                },
                body: JSON.stringify({ orderId: orderId })
            });

            const data = await response.json();

            if (data.success) {
                showNotification('Order deleted successfully.', 'success');
                loadPendingOrders();
            } else {
                showNotification('Error: ' + data.error, 'error');
            }
        } catch (error) {
            console.error('Error deleting order:', error);
            showNotification('Network error. Please try again.', 'error');
        }
    }

    // ===== MODAL MANAGEMENT =====
    function showModal(content) {
        const existingModal = document.getElementById('custom-modal');
        if (existingModal) existingModal.remove();

        const modalHtml = `
            <div id="custom-modal" class="modal-overlay">
                <div class="modal-container">
                    ${content}
                </div>
            </div>
        `;
        document.body.insertAdjacentHTML('beforeend', modalHtml);
    }

    function closeModal() {
        const modal = document.getElementById('custom-modal');
        if (modal) modal.remove();
    }

    async function viewOrderDetails(orderId) {
        try {
            console.log('Loading order details for:', orderId);
            const response = await fetch(`@Url.Action("GetOrderDetails", "Orders")?orderId=${orderId}`);
            const data = await response.json();

            if (data.success) {
                const order = data.order;
                console.log('Order data:', order);

                let detailsHtml = `
                    <div class="modal-content">
                        <div class="modal-header">
                            <h3 class="modal-title">Order Details: ${order.orderNumber}</h3>
                        </div>

                        <div class="info-section">
                            <p><strong>Supplier:</strong> ${order.supplier}</p>
                            <p><strong>Order Date:</strong> ${formatDate(order.orderDate)}</p>
                            <p><strong>Status:</strong> <span class="stock-status status-${order.status.toLowerCase()}">${order.status}</span></p>
                            <p><strong>Total Quantity:</strong> ${order.totalQuantity}</p>
                            <p><strong>Total Cost:</strong> R ${order.totalCost ? order.totalCost.toFixed(2) : '0.00'}</p>
                        </div>

                        <h4 style="margin-bottom: 15px; color: #343a40; font-weight: 600;">Medications:</h4>
                        <div style="max-height: 300px; overflow-y: auto;">
                            <table class="medications-table">
                                <thead>
                                    <tr>
                                        <th>Medication</th>
                                        <th class="text-center">Quantity</th>
                                        <th class="text-right">Unit Price</th>
                                        <th class="text-right">Total</th>
                                    </tr>
                                </thead>
                                <tbody>
                `;

                if (order.medications && order.medications.length > 0) {
                    order.medications.forEach(med => {
                        detailsHtml += `
                            <tr>
                                <td>${med.name}</td>
                                <td class="text-center">${med.quantity}</td>
                                <td class="text-right">R ${med.unitPrice ? med.unitPrice.toFixed(2) : '0.00'}</td>
                                <td class="text-right">R ${med.totalPrice ? med.totalPrice.toFixed(2) : '0.00'}</td>
                            </tr>
                        `;
                    });
                } else {
                    detailsHtml += `
                        <tr>
                            <td colspan="4" class="text-center" style="padding: 20px; color: #6c757d;">
                                <i class="fas fa-box-open" style="font-size: 24px; margin-bottom: 10px; display: block;"></i>
                                No medications found in this order
                            </td>
                        </tr>
                    `;
                }

                detailsHtml += `</tbody></table></div>`;

                // Add action buttons for pending orders
                if (order.status === 'Pending') {
                    detailsHtml += `
                        <div class="modal-actions">
                            <div class="action-buttons">
                                <button onclick="showReceiveOrderModal(${order.orderId})" class="modal-btn-primary">
                                    <i class="fas fa-check"></i> Receive Order
                                </button>
                                <button onclick="deleteOrderFromModal(${order.orderId})" class="modal-btn-danger">
                                    <i class="fas fa-trash"></i> Delete Order
                                </button>
                            </div>
                            <button onclick="closeModal()" class="modal-btn-secondary">
                                <i class="fas fa-times"></i> Close
                            </button>
                        </div>
                    `;
                } else {
                    detailsHtml += `
                        <div style="text-align: center; margin-top: 25px; padding-top: 20px; border-top: 1px solid #dee2e6;">
                            <p style="color: #6c757d; font-style: italic; margin-bottom: 20px;">
                                <i class="fas fa-info-circle"></i>
                                This order has been ${order.status.toLowerCase()} and cannot be modified.
                            </p>
                            <button onclick="closeModal()" class="modal-btn-secondary">
                                <i class="fas fa-times"></i> Close
                            </button>
                        </div>
                    `;
                }

                detailsHtml += `</div>`;

                showModal(detailsHtml);
            } else {
                showNotification('Error loading order details: ' + data.error, 'error');
            }
        } catch (error) {
            console.error('Error loading order details:', error);
            showNotification('Error loading order details.', 'error');
        }
    }

    async function showReceiveOrderModal(orderId) {
        try {
            const response = await fetch(`@Url.Action("GetOrderSummary", "Orders")?orderId=${orderId}`);
            const data = await response.json();

            if (data.success) {
                const order = data.order;
                let modalHtml = `
                    <div class="modal-content">
                        <div class="modal-header">
                            <h3 class="modal-title">Receive Order: ${order.orderNumber}</h3>
                        </div>

                        <div class="info-section">
                            <p><strong>Supplier:</strong> ${order.supplier}</p>
                            <p><strong>Order Date:</strong> ${formatDate(order.orderDate)}</p>
                            <p><strong>Total Quantity:</strong> ${order.totalQuantity}</p>
                            <p><strong>Total Cost:</strong> R ${order.totalCost.toFixed(2)}</p>
                        </div>

                        <h4 style="margin-bottom: 15px; color: #343a40; font-weight: 600;">Medications to Receive:</h4>
                        <div style="max-height: 300px; overflow-y: auto;">
                            <table class="medications-table">
                                <thead>
                                    <tr>
                                        <th>Medication</th>
                                        <th class="text-center">Order Qty</th>
                                        <th class="text-center">Current Stock</th>
                                        <th class="text-center">New Stock</th>
                                        <th class="text-right">Total</th>
                                    </tr>
                                </thead>
                                <tbody>
                `;

                order.medications.forEach(med => {
                    const newStock = med.currentStock + med.quantity;
                    modalHtml += `
                        <tr>
                            <td>${med.name}</td>
                            <td class="text-center">${med.quantity}</td>
                            <td class="text-center">${med.currentStock}</td>
                            <td class="text-center highlight-stock">${newStock}</td>
                            <td class="text-right">R ${med.totalPrice.toFixed(2)}</td>
                        </tr>
                    `;
                });

                modalHtml += `
                                </tbody>
                            </table>
                        </div>

                        <div class="warning-banner">
                            <p>
                                <i class="fas fa-info-circle"></i>
                                Receiving this order will update medication stock quantities and move the order to completed orders.
                            </p>
                        </div>

                        <div class="modal-actions">
                            <div></div>
                            <div class="action-buttons">
                                <button onclick="closeModal()" class="modal-btn-secondary">
                                    <i class="fas fa-times"></i> Cancel
                                </button>
                                <button onclick="confirmReceiveOrder(${order.orderId})" class="modal-btn-primary" id="confirm-receive-btn">
                                    <i class="fas fa-check"></i> Receive Order
                                </button>
                            </div>
                        </div>
                    </div>
                `;

                showModal(modalHtml);
            } else {
                showNotification('Error loading order summary: ' + data.error, 'error');
            }
        } catch (error) {
            showNotification('Error loading order summary.', 'error');
        }
    }

    async function confirmReceiveOrder(orderId) {
        const confirmBtn = document.getElementById('confirm-receive-btn');
        const originalText = confirmBtn.innerHTML;

        try {
            confirmBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Receiving...';
            confirmBtn.disabled = true;

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const response = await fetch('@Url.Action("ReceiveOrder", "Orders")', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token || ''
                },
                body: JSON.stringify({ orderId: orderId })
            });

            const data = await response.json();

            if (data.success) {
                showNotification('Order received successfully! Stock updated.', 'success');
                closeModal();
                await loadMedications();
                document.querySelector('[data-tab="completed-orders"]').click();
            } else {
                showNotification('Error: ' + data.error, 'error');
                confirmBtn.innerHTML = originalText;
                confirmBtn.disabled = false;
            }
        } catch (error) {
            showNotification('Network error. Please try again.', 'error');
            confirmBtn.innerHTML = originalText;
            confirmBtn.disabled = false;
        }
    }

    async function deleteOrderFromModal(orderId) {
        if (!confirm('Are you sure you want to delete this order? This action cannot be undone.')) {
            return;
        }

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const response = await fetch('@Url.Action("DeleteOrder", "Orders")', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token || ''
                },
                body: JSON.stringify({ orderId: orderId })
            });

            const data = await response.json();

            if (data.success) {
                showNotification('Order deleted successfully.', 'success');
                closeModal();
                loadPendingOrders();
            } else {
                showNotification('Error: ' + data.error, 'error');
            }
        } catch (error) {
            console.error('Error deleting order:', error);
            showNotification('Network error. Please try again.', 'error');
        }
    }

    // ===== UTILITY FUNCTIONS =====
    function getSelectedItems() {
        const selectedItems = [];
        document.querySelectorAll('.med-checkbox:checked').forEach(checkbox => {
            const row = checkbox.closest('tr');
            const qtyInput = row.querySelector('.order-qty');
            selectedItems.push({
                medicationId: parseInt(checkbox.getAttribute('data-id')),
                name: row.cells[1].textContent,
                quantity: parseInt(qtyInput.value) || 0,
                unitPrice: parseFloat(checkbox.getAttribute('data-price'))
            });
        });
        return selectedItems;
    }

    function showSupplierValidationWarning() {
        document.getElementById('supplier-validation').classList.add('show');
    }

    function hideSupplierValidationWarning() {
        document.getElementById('supplier-validation').classList.remove('show');
    }

    function formatDate(dateString) {
        if (!dateString || dateString === 'Not received') return dateString;
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
        } catch {
            return dateString;
        }
    }

    function showLoading(elementId, show) {
        const element = document.getElementById(elementId);
        if (element) element.style.display = show ? 'block' : 'none';
    }

    function showNotification(message, type = 'success') {
        const notification = document.getElementById('notification-banner');
        const messageElement = document.getElementById('notification-message');

        messageElement.textContent = message;
        notification.className = `notification-banner notification-${type}`;
        notification.classList.add('show');

        setTimeout(hideNotification, 5000);
    }

    function hideNotification() {
        document.getElementById('notification-banner').classList.remove('show');
