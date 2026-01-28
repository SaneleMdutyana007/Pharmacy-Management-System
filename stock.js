function filterStocks() {
    const supplierFilter = document.getElementById('supplierFilter').value;
    const statusFilter = document.getElementById('statusFilter').value;

    // Get base URL from data attribute
    const baseUrl = document.getElementById('filterSection').getAttribute('data-base-url');
    const params = [];

    if (supplierFilter !== 'all') {
        params.push(`supplierFilter=${supplierFilter}`);
    }

    if (statusFilter !== 'all') {
        params.push(`statusFilter=${statusFilter}`);
    }

    let finalUrl = baseUrl;
    if (params.length > 0) {
        finalUrl += '?' + params.join('&');
    }

    window.location.href = finalUrl;
}
function openAdjustModal(medicationId, medicationName, currentQuantity) {
    document.getElementById('adjustMedicationId').value = medicationId;
    document.getElementById('adjustMedicationName').textContent = medicationName;
    document.getElementById('currentQuantity').textContent = currentQuantity;
    document.getElementById('adjustmentType').value = 'add';
    document.getElementById('adjustQuantity').value = '';

    // Reset button styles
    document.querySelectorAll('#adjustStockModal .btn').forEach(btn => {
        btn.classList.remove('btn-primary');
        btn.classList.add('btn-secondary');
    });
    document.querySelector('#adjustStockModal .btn:first-child').classList.add('btn-primary');

    new bootstrap.Modal(document.getElementById('adjustStockModal')).show();
}

function setAdjustmentType(type) {
    document.getElementById('adjustmentType').value = type;

    // Update button styles
    document.querySelectorAll('#adjustStockModal .btn').forEach(btn => {
        btn.classList.remove('btn-primary');
        btn.classList.add('btn-secondary');
    });
    event.target.classList.remove('btn-secondary');
    event.target.classList.add('btn-primary');
}

// Handle AJAX form submission for stock adjustment
$(document).on('submit', '#adjustStockForm', function (e) {
    e.preventDefault();

    var form = $(this);
    var url = form.attr('action');
    var formData = form.serialize();

    $.ajax({
        url: url,
        type: 'POST',
        data: formData,
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            window.location.reload();
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
            alert('Error adjusting stock. Please try again.');
        }
    });
});