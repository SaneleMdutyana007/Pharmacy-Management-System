document.addEventListener('DOMContentLoaded', function () {
    // ========== DOM Elements ==========
    const elements = {
        addButton: document.getElementById('add-pharmacist'),
        searchBtn: document.getElementById('search-btn'),
        prevPageBtn: document.getElementById('prev-page'),
        nextPageBtn: document.getElementById('next-page'),
        submitBtn: document.getElementById('submit-btn'),
        cancelModal: document.getElementById('cancel-pharmacist-modal'),
        closeModal: document.querySelectorAll('.close-modal'),
        closeNotification: document.getElementById('close-notification'),
        form: document.getElementById('pharmacist-form'),
        searchInput: document.getElementById('search-pharmacist'),
        nameInput: document.getElementById('pharmacist-name'),
        idInput: document.getElementById('ID-number'),
        regInput: document.getElementById('pharmacist-health'),
        emailInput: document.getElementById('pharmacist-email'),
        contactInput: document.getElementById('ContactNumber'),
        nameError: document.getElementById('name-error'),
        idError: document.getElementById('id-error'),
        regError: document.getElementById('reg-error'),
        emailError: document.getElementById('email-error'),
        contactError: document.getElementById('contact-error'),
        modal: document.getElementById('pharmacist-modal'),
        modalTitle: document.getElementById('pharmacist-modal-title'),
        tableBody: document.getElementById('pharmacists-tbody'),
        pageInfo: document.getElementById('page-info'),
        notification: document.getElementById('notification'),
        notificationMessage: document.getElementById('notification-message'),
        addSuccessModal: document.getElementById('add-success-modal'),
        resetSuccessModal: document.getElementById('reset-success-modal')
    };
    const modals = {
        pharmacist: document.getElementById('pharmacist-modal'),
        addSuccess: document.getElementById('add-success-modal'),
        resetSuccess: document.getElementById('reset-success-modal')
    };

    // ========== App State ==========
    const state = {
        currentEditingId: null,
        currentPage: 1,
        rowsPerPage: 5,
        allPharmacists: [],
        filteredPharmacists: []
    };

    // ========== Password Utilities ==========
    function generateTempPassword() {
        const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*';
        let password = '';
        for (let i = 0; i < 10; i++) {
            password += chars.charAt(Math.floor(Math.random() * chars.length));
        }
        return password;
    }

    function generateVerificationCode() {
        return Math.floor(100000 + Math.random() * 900000);
    }

    // ========== Modal Management ==========
    function openModal(modal) {
        document.documentElement.style.overflow = 'hidden'; // Prevent background scrolling
        modal.classList.add('active');
        modal.setAttribute('aria-hidden', 'false');

        // Focus on first interactive element
        const focusTarget = modal.querySelector('input, button');
        if (focusTarget) focusTarget.focus();
    }


    // ========== Data Management ==========
    function loadPharmacists() {
        try {
            const storedPharmacists = localStorage.getItem('pharmacyPharmacists');
            if (storedPharmacists) {
                state.allPharmacists = JSON.parse(storedPharmacists);
            } else {
                state.allPharmacists = [
                    {
                        id: 1,
                        name: 'Lindile hadebe',
                        regNumber: '123456',
                        email: 'lindile@example.com',
                        contact: '0612345678',
                        password: 'Hadebe123!'
                    },
                    {
                        id: 2,
                        name: 'Dorothy Daniels',
                        regNumber: '234567',
                        email: 'dorothy@example.com',
                        contact: '0622345678',
                        password: 'Daniels123!'
                    },
                    {
                        id: 3,
                        name: 'Marcel Van Niekerk',
                        regNumber: '345678',
                        email: 'marcel@example.com',
                        contact: '0632345678',
                        password: 'VanNiekerk123!'
                    },
                    {
                        id: 4,
                        name: 'Olothando Mvango',
                        regNumber: '134679',
                        email: 'olothando@example.com',
                        contact: '0622345678',
                        password: 'Mvango123!'
                    }

                ];
                savePharmacists();
            }
            state.filteredPharmacists = [...state.allPharmacists];
            renderPharmacists();
        } catch (error) {
            showNotification('Error loading pharmacists', 'error');
            console.error('Error loading pharmacists:', error);
        }
    }

    function savePharmacists() {
        try {
            localStorage.setItem('pharmacyPharmacists', JSON.stringify(state.allPharmacists));
        } catch (error) {
            showNotification('Error saving pharmacists', 'error');
            console.error('Error saving pharmacists:', error);
        }
    }

    function generateId() {
        return state.allPharmacists.length > 0
            ? Math.max(...state.allPharmacists.map(p => p.id)) + 1
            : 1;
    }

    // ========== Rendering ==========
    function renderPharmacists() {
        elements.tableBody.innerHTML = '';
        const startIdx = (state.currentPage - 1) * state.rowsPerPage;
        const endIdx = startIdx + state.rowsPerPage;
        const pharmacistsToShow = state.filteredPharmacists.slice(startIdx, endIdx);

        pharmacistsToShow.forEach(pharmacist => {
            const row = document.createElement('tr');
            row.innerHTML = `
                    <td>${pharmacist.id}</td>
                    <td>${pharmacist.name}</td>
                    <td>${pharmacist.regNumber}</td>
                    <td>${pharmacist.email}</td>
                    <td>${pharmacist.contact}</td>
                    <td class="actions-cell">
                        <button class="btn-action btn-edit" data-id="${pharmacist.id}">
                            <i class="fas fa-edit"></i> Edit
                        </button>
                        <br><br/>
                        <button class="btn-action btn-delete" data-id="${pharmacist.id}">
                            Delete
                        </button>
                        <br><br/>
                        <button class="btn-action btn-reset" data-id="${pharmacist.id}">
                            Reset
                        </button>
                    </td>
                `;
            elements.tableBody.appendChild(row);
        });

        updatePagination();
    }

    function updatePagination() {
        const totalPages = Math.ceil(state.filteredPharmacists.length / state.rowsPerPage) || 1;
        elements.pageInfo.textContent = `Page ${state.currentPage} of ${totalPages}`;
        elements.prevPageBtn.disabled = state.currentPage <= 1;
        elements.nextPageBtn.disabled = state.currentPage >= totalPages;
    }

    // ========== Event Handlers ==========
    function setupEventListeners() {
        elements.addButton.addEventListener('click', openAddModal);
        elements.form.addEventListener('submit', handleFormSubmit);
        elements.tableBody.addEventListener('click', handleTableAction);
        elements.searchBtn.addEventListener('click', performSearch);
        elements.addButton.addEventListener('click', () => {
            openModal(modals.pharmacist);
        });

        elements.searchInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') performSearch();
        });

        elements.prevPageBtn.addEventListener('click', () => {
            if (state.currentPage > 1) {
                state.currentPage--;
                renderPharmacists();
            }
        });

        elements.nextPageBtn.addEventListener('click', () => {
            const totalPages = Math.ceil(state.filteredPharmacists.length / state.rowsPerPage);
            if (state.currentPage < totalPages) {
                state.currentPage++;
                renderPharmacists();
            }
        });

        document.getElementById('cancel-pharmacist-modal').addEventListener('click', closeAllModals);




        document.querySelectorAll('.close-modal, .modal-actions button').forEach(btn => {
            btn.addEventListener('click', closeAllModals);
        });

        // Overlay Click
        Object.values(modals).forEach(modal => {
            modal.addEventListener('click', (e) => {
                if (e.target === modal) closeAllModals();
            });
        });

        // Escape Key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') closeAllModals();
        });
    }

    function openAddModal() {
        elements.modalTitle.textContent = 'Add New Pharmacist';
        elements.submitBtn.innerHTML = '<i class="fas fa-save"></i> Save';
        clearFormErrors();
        elements.form.reset();
        state.currentEditingId = null;
        openModal(modals.pharmacist);
        elements.nameInput.focus();
    }

    function handleFormSubmit(e) {
        e.preventDefault();

        const name = elements.nameInput.value.trim();
        const regNumber = elements.regInput.value.trim();
        const email = elements.emailInput.value.trim();
        const contact = elements.contactInput.value.trim();

        let isValid = true;
        clearFormErrors();

        if (!name) {
            elements.nameError.textContent = 'Please enter a name';
            isValid = false;
        }

       

        if (regNumber) {
            elements.regError.textContent = 'Registration number should be 6 digits';
            isValid = false;
        }

        if (!email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            elements.emailError.textContent = 'Please enter a valid email address';
            isValid = false;
        }

        if (!/^\d{10}$/.test(contact)) {
            elements.contactError.textContent = 'Please enter a valid 10-digit phone number';
            isValid = false;
        }

        if (!isValid) return;

        if (state.currentEditingId) {
            const index = state.allPharmacists.findIndex(p => p.id === state.currentEditingId);
            if (index !== -1) {
                state.allPharmacists[index] = {
                    ...state.allPharmacists[index],
                    name,
                    regNumber,
                    email,
                    contact,

                };
                savePharmacists();

                state.filteredPharmacists = [...state.allPharmacists];
                renderPharmacists();

                closeAllModals();

            }
        } else {
            const tempPassword = generateTempPassword();
            const newId = generateId();
            state.allPharmacists.push({
                id: newId,
                name,
                regNumber,
                email,
                contact,
                password: tempPassword
            });
            savePharmacists();

            closeAllModals();

            // Then show success modal
            document.getElementById('success-name').textContent = name;
            document.getElementById('success-email').textContent = email;
            openModal(modals.addSuccess);

            // Refresh display
            state.filteredPharmacists = [...state.allPharmacists];
            renderPharmacists();

        }
        elements.form.reset();
        state.currentEditingId = null;
        performSearch(elements.searchInput.value.trim());

    }
    function closeAllModals() {
        document.documentElement.style.overflow = '';
        Object.values(modals).forEach(modal => {
            modal.classList.remove('active');
            modal.setAttribute('aria-hidden', 'true');
        });
    }

    function handleTableAction(e) {
        const target = e.target.closest('button');
        if (!target) return;

        const id = parseInt(target.getAttribute('data-id'));
        if (target.classList.contains('btn-delete')) {
            deletePharmacist(id);
        } else if (target.classList.contains('btn-edit')) {
            openEditModal(id);
        } else if (target.classList.contains('btn-reset')) {
            resetPassword(id);
        }
    }

    function openEditModal(id) {
        const pharmacist = state.allPharmacists.find(p => p.id === id);
        if (pharmacist) {
            elements.nameInput.value = pharmacist.name;
            elements.regInput.value = pharmacist.regNumber;
            elements.emailInput.value = pharmacist.email;
            elements.contactInput.value = pharmacist.contact;

            elements.modalTitle.textContent = 'Edit Pharmacist';
            elements.submitBtn.innerHTML = '<i class="fas fa-save"></i> Update';
            clearFormErrors();
            state.currentEditingId = id;
            openModal(modals.pharmacist);
            elements.nameInput.focus();

        }
    }

    function deletePharmacist(id) {
        // if (confirm('Are you sure you want to delete this pharmacist?')) {
        state.allPharmacists = state.allPharmacists.filter(p => p.id !== id);//
        savePharmacists();
        performSearch(elements.searchInput.value.trim());
        showNotification('Pharmacist deleted successfully!', 'success');

    }

    function resetPassword(id) {
        const pharmacist = state.allPharmacists.find(p => p.id === id);
        if (pharmacist && confirm(`Send password reset link to ${pharmacist.email}?`)) {
            const newPassword = generateTempPassword();
            const verificationCode = generateVerificationCode();

            pharmacist.password = newPassword;
            savePharmacists();

            document.getElementById('reset-email').textContent = pharmacist.email;
            document.getElementById('reset-code').textContent = verificationCode;
            openModal(modals.resetSuccess);
        }

    }

    // ========== Helper Functions ==========
    function performSearch() {
        const searchTerm = elements.searchInput.value.trim().toLowerCase();

        state.filteredPharmacists = searchTerm ?
            state.allPharmacists.filter(p =>
                p.name.toLowerCase().includes(searchTerm) ||
                p.regNumber.toLowerCase().includes(searchTerm) ||
                p.email.toLowerCase().includes(searchTerm) ||
                p.contact.includes(searchTerm)) :
            [...state.allPharmacists];


        state.currentPage = 1;
        renderPharmacists();

    }

    function clearFormErrors() {
        Object.values(elements).forEach(element => {
            if (element.id && element.id.endsWith('-error')) {
                element.textContent = '';
            }
        });
    }

    function showNotification(message, type = 'success') {
        elements.notificationMessage.textContent = message;
        elements.notification.className = `notification ${type}`;
        elements.notification.style.display = 'flex';
        setTimeout(() => {
            elements.notification.style.display = 'none';
        }, 3000);
    }

    // ========== Initialization ==========
    function init() {
        loadPharmacists();
        setupEventListeners();
        Object.values(modals).forEach(modal => {
            modal.setAttribute('aria-modal', 'true');
            modal.setAttribute('aria-hidden', 'true');
        });
    }

    init();
});