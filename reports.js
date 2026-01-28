// Initialize jsPDF
const { jsPDF } = window.jspdf;

document.addEventListener('DOMContentLoaded', function () {
    // Set current date
    const now = new Date();
    document.getElementById('report-date').textContent = now.toLocaleDateString('en-ZA', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });

    // Load medications from localStorage
    let medications = [];
    const storedMedications = localStorage.getItem('pharmacyMedications');
    if (storedMedications) {
        medications = JSON.parse(storedMedications);
    } else {
        // Default medications if none exist
        medications = [
            {
                id: 1,
                name: 'Panadol',
                schedule: '2',
                dosageForm: 'Tablet',
                price: '15.99',
                ingredients: 'Paracetamol 500mg',
                supplier: 'PharmaPlus',
                quantity: '24',
                reorderLevel: '10'
            },
            {
                id: 2,
                name: 'Amoxil',
                schedule: '0',
                dosageForm: 'Tablet',
                price: '8.50',
                ingredients: 'Amoxicillin 250mg',
                supplier: 'MediCorp',
                quantity: '8',
                reorderLevel: '10'
            },
            {
                id: 3,
                name: 'Advil',
                schedule: '2',
                dosageForm: 'Syrup',
                price: '34.00',
                ingredients: 'Ibuprofen',
                supplier: 'HealthSupplies',
                quantity: '12',
                reorderLevel: '10'
            },


        ];
    }

    // Generate report button
    document.getElementById('generate-btn').addEventListener('click', function () {
        generateReport();
    });

    // Print PDF button
    document.getElementById('print-pdf').addEventListener('click', function () {
        generatePDF();
    });

    function generateReport() {
        const groupBy = document.getElementById('group-by').value;
        const filterSupplier = document.getElementById('filter-supplier').value;
        const filterSchedule = document.getElementById('filter-schedule').value;
        const filterDosage = document.getElementById('filter-dosage').value;

        // Filter medications
        let filteredMeds = [...medications];

        if (filterSupplier !== 'all') {
            filteredMeds = filteredMeds.filter(m => m.supplier === filterSupplier);
        }

        if (filterSchedule !== 'all') {
            filteredMeds = filteredMeds.filter(m => m.schedule === filterSchedule);
        }

        if (filterDosage !== 'all') {
            filteredMeds = filteredMeds.filter(m => m.dosageForm === filterDosage);
        }

        // Sort medications
        if (groupBy === 'none') {
            filteredMeds.sort((a, b) => a.name.localeCompare(b.name));
        } else if (groupBy === 'dosage') {
            filteredMeds.sort((a, b) => a.dosageForm.localeCompare(b.dosageForm) || a.name.localeCompare(b.name));
        } else if (groupBy === 'schedule') {
            filteredMeds.sort((a, b) => a.schedule.localeCompare(b.schedule) || a.name.localeCompare(b.name));
        } else if (groupBy === 'supplier') {
            filteredMeds.sort((a, b) => a.supplier.localeCompare(b.supplier) || a.name.localeCompare(b.name));
        }

        // Generate HTML report
        const reportBody = document.getElementById('report-body');
        reportBody.innerHTML = '';

        if (filteredMeds.length === 0) {
            const row = document.createElement('tr');
            row.innerHTML = `<td colspan="9" style="text-align: center;">No medications match the selected filters</td>`;
            reportBody.appendChild(row);
            return;
        }

        let currentGroup = '';

        filteredMeds.forEach(med => {
            // Add group header if needed
            let groupValue = '';
            if (groupBy === 'dosage') {
                groupValue = med.dosageForm;
            } else if (groupBy === 'schedule') {
                groupValue = `Schedule ${med.schedule}`;
            } else if (groupBy === 'supplier') {
                groupValue = med.supplier;
            }

            if (groupBy !== 'none' && groupValue !== currentGroup) {
                currentGroup = groupValue;
                const groupRow = document.createElement('tr');
                groupRow.className = 'group-header';
                groupRow.innerHTML = `<td colspan="9">${groupValue}</td>`;
                reportBody.appendChild(groupRow);
            }

            // Add medication row
            const row = document.createElement('tr');
            const status = parseInt(med.quantity) < parseInt(med.reorderLevel) ? 'Low Stock' : 'OK';
            const statusClass = status === 'Low Stock' ? 'low-stock' : '';

            row.innerHTML = `
                            <td>${med.id}</td>
                            <td>${med.name}</td>
                            <td>${med.schedule}</td>
                            <td>${med.dosageForm}</td>
                            <td>${med.ingredients}</td>
                            <td>${med.supplier}</td>
                            <td class="${statusClass}">${med.quantity}</td>
                            <td>${med.reorderLevel}</td>
                            <td class="${statusClass}">${status}</td>
                        `;
            reportBody.appendChild(row);
        });
    }

    function generatePDF() {
        const doc = new jsPDF();
        const groupBy = document.getElementById('group-by').value;
        const now = new Date();

        // Add header
        doc.setFontSize(16);
        doc.setTextColor(40, 40, 40);
        doc.text('Ibhayi Pharmacy', 105, 15, { align: 'center' });

        doc.setFontSize(12);
        doc.setTextColor(100, 100, 100);
        doc.text('123 Main Street, Ibhayi, Eastern Cape, 6200', 105, 22, { align: 'center' });
        doc.text('Tel: 041 123 4567 | Email: info@ibhayipharmacy.co.za', 105, 28, { align: 'center' });

        // Add title
        doc.setFontSize(14);
        doc.setTextColor(40, 40, 40);
        doc.text('Medication Stock Take Report', 105, 38, { align: 'center' });

        // Add report details
        doc.setFontSize(10);
        doc.setTextColor(100, 100, 100);
        let reportDetails = `Generated by Sanele Mdutyana on: ${now.toLocaleDateString('en-ZA')}`;
        reportDetails += ` | Grouped by: ${document.getElementById('group-by').options[document.getElementById('group-by').selectedIndex].text}`;
        reportDetails += ` | Filter: ${getFiltersText()}`;
        doc.text(reportDetails, 105, 45, { align: 'center' });

        // Prepare data for the table
        const headers = [
            'ID',
            'Name',
            'Schedule',
            'Dosage Form',
            'Ingredients',
            'Supplier',
            'Stock',
            'Reorder',
            'Status'
        ];

        const rows = [];
        let currentGroup = '';
        const reportRows = document.getElementById('report-body').rows;

        for (let i = 0; i < reportRows.length; i++) {
            const row = reportRows[i];

            if (row.className === 'group-header') {
                // Add group header
                currentGroup = row.cells[0].textContent;
                doc.setFontSize(12);
                doc.setTextColor(255, 255, 255);
                doc.setFillColor(44, 62, 80);
                doc.rect(10, doc.autoTable.previous.finalY + 10 || 55, 190, 8, 'F');
                doc.text(currentGroup, 14, doc.autoTable.previous.finalY + 15 || 60);
                continue;
            }

            const rowData = [];
            for (let j = 0; j < row.cells.length; j++) {
                rowData.push(row.cells[j].textContent);
            }

            // Add row color for low stock
            const isLowStock = row.cells[8].textContent === 'Low Stock';
            rows.push({
                data: rowData,
                styles: {
                    fillColor: isLowStock ? [255, 230, 230] : undefined,
                    textColor: isLowStock ? [231, 76, 60] : undefined
                }
            });
        }

        // Add the table
        doc.autoTable({
            startY: doc.autoTable.previous.finalY + 20 || 65,
            head: [headers],
            body: rows.map(r => r.data),
            styles: {
                fontSize: 9,
                cellPadding: 3,
                overflow: 'linebreak'
            },
            columnStyles: {
                0: { cellWidth: 10 }, // ID
                1: { cellWidth: 30 }, // Name
                2: { cellWidth: 15 }, // Schedule
                3: { cellWidth: 20 }, // Dosage Form
                4: { cellWidth: 40 }, // Ingredients
                5: { cellWidth: 25 }, // Supplier
                6: { cellWidth: 15 }, // Stock
                7: { cellWidth: 15 }, // Reorder
                8: { cellWidth: 15 }  // Status
            },
            didParseCell: function (data) {
                // Apply custom styles for specific rows
                if (data.row.index > 0) { // Skip header row
                    const rowStyle = rows[data.row.index - 1].styles;
                    if (rowStyle) {
                        data.cell.styles.fillColor = rowStyle.fillColor;
                        data.cell.styles.textColor = rowStyle.textColor;
                    }
                }
            }
        });

        // Add footer
        doc.setFontSize(10);
        doc.setTextColor(100, 100, 100);
        doc.text(`Report generated by Sanele Mdutyana (Manager) on ${now.toLocaleDateString('en-ZA', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        })}`, 105, doc.internal.pageSize.height - 10, { align: 'center' });

        // Save the PDF
        doc.save(`Ibhayi_Pharmacy_Stock_Report_${now.toISOString().slice(0, 10)}.pdf`);
    }

    function getFiltersText() {
        const filters = [];
        const filterSupplier = document.getElementById('filter-supplier').value;
        const filterSchedule = document.getElementById('filter-schedule').value;
        const filterDosage = document.getElementById('filter-dosage').value;

        if (filterSupplier !== 'all') filters.push(`Supplier: ${filterSupplier}`);
        if (filterSchedule !== 'all') filters.push(`Schedule: ${filterSchedule}`);
        if (filterDosage !== 'all') filters.push(`Dosage: ${filterDosage}`);

        return filters.length > 0 ? filters.join(', ') : 'No filters';
    }

    // Generate initial report
    generateReport();
});
 