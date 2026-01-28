// PharmacyDetails specific JavaScript
console.log('PharmacyDetails script loaded');

document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded, initializing pharmacy form...');

    // Initialize form validation
    initializeFormValidation();

    // Check for success/error messages
    checkForMessages();
});

function initializeFormValidation() {
    // Bootstrap form validation
    const forms = document.querySelectorAll('.needs-validation');

    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }

            form.classList.add('was-validated');
        }, false);
    });

    // Real-time validation for specific fields
    const vatInput = document.querySelector('input[name="VAT"]');
    if (vatInput) {
        vatInput.addEventListener('input', function () {
            const value = parseFloat(this.value);
            if (value < 0 || value > 100) {
                this.setCustomValidity('VAT must be between 0 and 100');
            } else {
                this.setCustomValidity('');
            }
        });
    }

    // Email validation
    const emailInput = document.querySelector('input[type="email"]');
    if (emailInput) {
        emailInput.addEventListener('input', function () {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(this.value)) {
                this.setCustomValidity('Please enter a valid email address');
            } else {
                this.setCustomValidity('');
            }
        });
    }
}

function checkForMessages() {
    // Auto-hide success messages after 5 seconds
    const successAlert = document.querySelector('.alert-success');
    if (successAlert) {
        setTimeout(() => {
            successAlert.style.opacity = '0';
            setTimeout(() => successAlert.remove(), 300);
        }, 5000);
    }

    // Add close button to error messages
    const errorAlerts = document.querySelectorAll('.alert-danger, .alert-info');
    errorAlerts.forEach(alert => {
        if (!alert.querySelector('.btn-close')) {
            const closeButton = document.createElement('button');
            closeButton.type = 'button';
            closeButton.className = 'btn-close';
            closeButton.setAttribute('data-bs-dismiss', 'alert');
            closeButton.setAttribute('aria-label', 'Close');
            alert.appendChild(closeButton);
        }
    });
}

// Helper function to format phone numbers
function formatPhoneNumber(input) {
    const phoneInput = document.querySelector('input[name="Contact"]');
    if (phoneInput) {
        phoneInput.addEventListener('input', function (e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length > 10) value = value.substring(0, 10);

            if (value.length >= 6) {
                value = value.replace(/(\d{3})(\d{3})(\d{4})/, '$1-$2-$3');
            } else if (value.length >= 3) {
                value = value.replace(/(\d{3})(\d{0,3})/, '$1-$2');
            }

            e.target.value = value;
        });
    }
}

// Export functions for potential reuse
window.PharmacyDetails = {
    initializeFormValidation,
    checkForMessages,
    formatPhoneNumber
};