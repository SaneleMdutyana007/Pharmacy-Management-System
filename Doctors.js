document.addEventListener('DOMContentLoaded', function () {
    // Edit button functionality
    const editButtons = document.querySelectorAll('.edit-btn');
    editButtons.forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const name = this.getAttribute('data-name');
            const surname = this.getAttribute('data-surname');
            const practice = this.getAttribute('data-practice');
            const phone = this.getAttribute('data-phone');
            const email = this.getAttribute('data-email');

            // Populate edit modal
            document.getElementById('editDoctorId').value = id;
            document.getElementById('editDoctorName').value = name;
            document.getElementById('editDoctorSurname').value = surname;
            document.getElementById('editPracticeNumber').value = practice;
            document.getElementById('editPhoneNumber').value = phone;
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
            document.getElementById('detailsDoctorName').textContent = row.cells[0].textContent;
            document.getElementById('detailsDoctorSurname').textContent = row.cells[1].textContent;
            document.getElementById('detailsPracticeNumber').textContent = row.cells[2].textContent;
            document.getElementById('detailsPhoneNumber').textContent = row.cells[3].textContent;
            document.getElementById('detailsEmail').textContent = row.cells[4].textContent;
        });
    });

    // Delete button functionality
    const deleteButtons = document.querySelectorAll('.delete-btn');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const name = this.getAttribute('data-name');

            document.getElementById('deleteDoctorId').value = id;
            document.getElementById('deleteDoctorName').textContent = name;
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