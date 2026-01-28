// Medications Management JavaScript

document.addEventListener('DOMContentLoaded', function () {
    initializeEventListeners();
    setupFormValidation();
    setupModalHandlers();
});

function initializeEventListeners() {
    // Edit button functionality
    const editButtons = document.querySelectorAll('.edit-btn');
    editButtons.forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            loadMedicationForEdit(id);
        });
    });

    // Details button functionality
    const detailsButtons = document.querySelectorAll('.details-btn');
    detailsButtons.forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const row = this.closest('tr');
            loadMedicationDetails(id, row);
        });
    });

    // Delete button functionality
    const deleteButtons = document.querySelectorAll('.delete-btn');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const name = this.getAttribute('data-name');
            setupDeleteModal(id, name);
        });
    });

    // Add ingredient buttons
    document.getElementById('addIngredientSelect')?.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            addIngredient('add');
        }
    });

    document.getElementById('editIngredientSelect')?.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            addIngredient('edit');
        }
    });
}

function setupFormValidation() {
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
}

function setupModalHandlers() {
    // Reset forms when modal is hidden
    const modals = ['addModal', 'editModal', 'detailsModal', 'deleteModal'];
    modals.forEach(modalId => {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.addEventListener('hidden.bs.modal', function () {
                const form = this.querySelector('form');
                if (form) {
                    form.classList.remove('was-validated');
                    if (modalId === 'addModal' || modalId === 'editModal') {
                        form.reset();
                        // Clear ingredient tables
                        const table = this.querySelector('table tbody');
                        if (table) table.innerHTML = '';
                    }
                }
            });

            // Reset ingredient tables when modal is shown (for add)
            if (modalId === 'addModal') {
                modal.addEventListener('show.bs.modal', function () {
                    const table = document.getElementById('addIngredientTable').getElementsByTagName('tbody')[0];
                    table.innerHTML = '';
                });
            }
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
}

// Medication Loading Functions
function loadMedicationForEdit(medicationId) {
    showLoadingState('editModal', true);

    fetch(`/Medications/GetMedicationWithIngredients/${medicationId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                populateEditForm(data);
            } else {
                showError('Error loading medication: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showError('Error loading medication data: ' + error.message);
        })
        .finally(() => {
            showLoadingState('editModal', false);
        });
}

function populateEditForm(data) {
    // Populate basic fields
    document.getElementById('editMedicationId').value = data.medicationId;
    document.getElementById('editMedicationName').value = data.medicationName || '';
    document.getElementById('editSchedule').value = data.schedule || '';
    document.getElementById('editDosageFormId').value = data.dosageFormId || '';
    document.getElementById('editPrice').value = data.price || '';
    document.getElementById('editSupplierId').value = data.supplierId || '';
    document.getElementById('editQuantity').value = data.quantity || '';
    document.getElementById('editReOrderLevel').value = data.reOrderLevel || '';

    // Populate ingredients table
    const table = document.getElementById('editIngredientTable').getElementsByTagName('tbody')[0];
    table.innerHTML = '';

    if (data.ingredients && data.ingredients.length > 0) {
        data.ingredients.forEach((ingredient, index) => {
            addIngredientRow(table, ingredient, index, 'edit');
        });
    }
}

function loadMedicationDetails(medicationId, row) {
    // Populate basic information from the row first (for faster display)
    document.getElementById('detailsMedicationName').textContent = row.cells[0].textContent;
    document.getElementById('detailsSchedule').textContent = row.cells[1].textContent;
    document.getElementById('detailsDosageForm').textContent = row.cells[2].textContent;
    document.getElementById('detailsPrice').textContent = row.cells[3].textContent;
    document.getElementById('detailsSupplier').textContent = row.cells[4].textContent;
    document.getElementById('detailsQuantity').textContent = row.cells[5].textContent;
    document.getElementById('detailsReOrderLevel').textContent = row.cells[6].textContent;

    // Then load ingredients via AJAX
    fetch(`/Medications/GetMedicationWithIngredients/${medicationId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                populateDetailsIngredients(data.ingredients);
            }
        })
        .catch(error => {
            console.error('Error loading details:', error);
        });
}

function populateDetailsIngredients(ingredients) {
    const table = document.getElementById('detailsIngredientTable').getElementsByTagName('tbody')[0];
    const noIngredientsMessage = document.getElementById('noIngredientsMessage');

    table.innerHTML = '';

    if (ingredients && ingredients.length > 0) {
        noIngredientsMessage.style.display = 'none';
        ingredients.forEach(ingredient => {
            const newRow = table.insertRow();
            newRow.innerHTML = `
                <td>${ingredient.ingredientName || 'Unknown'}</td>
                <td>${ingredient.strength || ''}</td>
            `;
        });
    } else {
        noIngredientsMessage.style.display = 'block';
    }
}

function setupDeleteModal(id, name) {
    document.getElementById('deleteMedicationId').value = id;
    document.getElementById('deleteMedicationName').textContent = name;
}

// Ingredient Management Functions
function addIngredient(modalType) {
    const select = document.getElementById(`${modalType}IngredientSelect`);
    const strengthInput = document.getElementById(`${modalType}IngredientStrength`);
    const table = document.getElementById(`${modalType}IngredientTable`).getElementsByTagName('tbody')[0];

    if (!select.value) {
        showError('Please select an active ingredient.');
        return;
    }

    if (!strengthInput.value.trim()) {
        showError('Please enter the strength.');
        return;
    }

    const ingredientId = select.value;
    const ingredientName = select.options[select.selectedIndex].text;
    const strength = strengthInput.value.trim();

    // Check if ingredient already exists
    if (isIngredientAlreadyAdded(table, ingredientId)) {
        showError('This ingredient has already been added.');
        return;
    }

    // Add new row
    const index = table.rows.length;
    addIngredientRow(table, {
        activeIngredientId: ingredientId,
        ingredientName: ingredientName,
        strength: strength
    }, index, modalType);

    // Clear inputs
    select.value = '';
    strengthInput.value = '';
}

function addIngredientRow(table, ingredient, index, modalType) {
    const newRow = table.insertRow();
    newRow.setAttribute('data-ingredient-id', ingredient.activeIngredientId);

    newRow.innerHTML = `
        <td>${ingredient.ingredientName}</td>
        <td>${ingredient.strength}</td>
        <td>
            <button type="button" class="btn btn-danger btn-sm" onclick="removeIngredient(this, '${modalType}')">
                <i class="fas fa-trash"></i> Remove
            </button>
            <input type="hidden" name="Ingredients[${index}].ActiveIngredientId" value="${ingredient.activeIngredientId}" />
            <input type="hidden" name="Ingredients[${index}].Strength" value="${ingredient.strength}" />
        </td>
    `;
}

function isIngredientAlreadyAdded(table, ingredientId) {
    const rows = table.getElementsByTagName('tr');
    for (let i = 0; i < rows.length; i++) {
        if (rows[i].getAttribute('data-ingredient-id') === ingredientId) {
            return true;
        }
    }
    return false;
}

function removeIngredient(button, modalType) {
    const row = button.closest('tr');
    row.remove();

    // Reindex the hidden inputs
    const table = document.getElementById(`${modalType}IngredientTable`).getElementsByTagName('tbody')[0];
    const rows = table.getElementsByTagName('tr');

    for (let i = 0; i < rows.length; i++) {
        const hiddenInputs = rows[i].getElementsByTagName('input');
        if (hiddenInputs.length >= 2) {
            hiddenInputs[0].name = `Ingredients[${i}].ActiveIngredientId`;
            hiddenInputs[1].name = `Ingredients[${i}].Strength`;
        }
    }
}

// Utility Functions
function showLoadingState(modalId, isLoading) {
    const modal = document.getElementById(modalId);
    const submitButton = modal?.querySelector('button[type="submit"]');

    if (submitButton) {
        if (isLoading) {
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Loading...';
        } else {
            submitButton.disabled = false;
            if (modalId === 'editModal') {
                submitButton.innerHTML = 'Save Changes';
            } else if (modalId === 'addModal') {
                submitButton.innerHTML = 'Add Medication';
            }
        }
    }
}

function showError(message) {
    alert(message); // You can replace this with a better notification system
}

function showSuccess(message) {
    alert(message); // You can replace this with a better notification system
}

// Handle delete form submission
document.addEventListener('DOMContentLoaded', function () {
    const deleteForm = document.getElementById('deleteForm');
    if (deleteForm) {
        deleteForm.addEventListener('submit', function (e) {
            e.preventDefault();

            if (confirm('Are you sure you want to delete this medication? This action cannot be undone.')) {
                this.submit();
            }
        });
    }
});