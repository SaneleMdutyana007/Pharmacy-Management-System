document.addEventListener('DOMContentLoaded', function () {
    // Edit button functionality
    const editButtons = document.querySelectorAll('.edit-btn');
    editButtons.forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const name = this.getAttribute('data-name');
            const contactPerson = this.getAttribute('data-contactperson');
            const email = this.getAttribute('data-email');

            // Populate edit modal
            document.getElementById('editSupplierId').value = id;
            document.getElementById('editSupplierName').value = name;
            document.getElementById('editContactPerson').value = contactPerson;
            document.getElementById('editEmail').value = email;
        });
    });

    // Details button functionality
    const detailsButtons = document.querySelectorAll('.details-btn');
    detailsButtons.forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const row = this.closest('tr');

            // Populate details modal
            document.getElementById('detailsSupplierName').textContent = row.cells[0].textContent;
            document.getElementById('detailsContactPerson').textContent = row.cells[1].textContent;
            document.getElementById('detailsEmail').textContent = row.cells[2].textContent;
        });
    });

    // Delete button functionality
    const deleteButtons = document.querySelectorAll('.delete-btn');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const name = this.getAttribute('data-name');

            document.getElementById('deleteSupplierId').value = id;
            document.getElementById('deleteSupplierName').textContent = name;
        });
    });

    // Handle delete form submission
    document.getElementById('deleteForm')?.addEventListener('submit', function (e) {
        e.preventDefault();
        const formData = new FormData(this);

        fetch(this.action, {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        }).then(response => {
            if (response.redirected) {
                window.location.href = response.url;
            }
        });
    });

    // Form validation
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

    // Reset forms when modal is hidden
    const modals = ['addModal', 'editModal', 'detailsModal'];
    modals.forEach(modalId => {
        const modal = document.getElementById(modalId);
        modal.addEventListener('hidden.bs.modal', function () {
            const form = this.querySelector('form');
            if (form) {
                form.classList.remove('was-validated');
                form.reset();
            }
        });
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);
});