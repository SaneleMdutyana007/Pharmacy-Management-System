document.addEventListener('DOMContentLoaded', function () {
    // Edit button functionality
    const editButtons = document.querySelectorAll('.edit-btn');
    editButtons.forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const name = this.getAttribute('data-name');
            const surname = this.getAttribute('data-surname');
            const email = this.getAttribute('data-email');
            const contactNumber = this.getAttribute('data-contactnumber');

            // Populate edit modal
            document.getElementById('editUserId').value = id;
            document.getElementById('editName').value = name;
            document.getElementById('editSurname').value = surname;
            document.getElementById('editEmail').value = email;
            document.getElementById('editContactNumber').value = contactNumber;
        });
    });

    // Details button functionality
    const detailsButtons = document.querySelectorAll('.details-btn');
    detailsButtons.forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const row = this.closest('tr');
            const cells = row.cells;

            // Populate details modal
            document.getElementById('detailsName').textContent = cells[0].textContent;
            document.getElementById('detailsSurname').textContent = cells[1].textContent;
            document.getElementById('detailsEmail').textContent = cells[2].textContent;
            document.getElementById('detailsContactNumber').textContent = cells[3].textContent;
            document.getElementById('detailsUserType').textContent = cells[4].textContent;
        });
    });

    // Delete button functionality
    const deleteButtons = document.querySelectorAll('.delete-btn');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const name = this.getAttribute('data-name');
            const surname = this.getAttribute('data-surname');

            document.getElementById('deleteUserId').value = id;
            document.getElementById('deleteUserName').textContent = name + ' ' + surname;
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
    const modals = ['editModal', 'detailsModal', 'deleteModal'];
    modals.forEach(modalId => {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.addEventListener('hidden.bs.modal', function () {
                const form = this.querySelector('form');
                if (form) {
                    form.classList.remove('was-validated');
                    form.reset();
                }
            });
        }
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