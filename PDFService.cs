using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Properties;
using PharmacyManager.Data;
using PharmacyManager.Models;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace PharmacyManager.Services
{
    public class PDFService
    {
        private readonly PharmacyDbContext _context;
        public PDFService(PharmacyDbContext context)
        {
            _context = context;
        }
        //public byte[] GenerateCustomerReport(
        //    string customerId,
        //    DateTime startDate,
        //    DateTime endDate,
        //    string groupBy = "doctor")
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        var writer = new PdfWriter(memoryStream);
        //        var pdf = new PdfDocument(writer);
        //        var document = new Document(pdf);

        //        // Title
        //        document.Add(new Paragraph("DISPENSED PRESCRIPTIONS")
        //            .SetFontSize(16)
        //            .SetTextAlignment(TextAlignment.CENTER));
        //        document.Add(new Paragraph($"Customer ID: {customerId}")
        //            .SetTextAlignment(TextAlignment.CENTER));
        //        document.Add(new Paragraph($"{startDate:dd/MM/yyyy} – {endDate:dd/MM/yyyy}")
        //            .SetTextAlignment(TextAlignment.CENTER));
        //        document.Add(new LineSeparator(new SolidLine()));
        //        document.Add(new Paragraph(" ")); // spacing

        //        // Fetch prescriptions
        //        var prescriptions = _context.Prescriptions
        //            .Where(p => p.CustomerId == customerId
        //                        && p.ScriptDate >= startDate
        //                        && p.ScriptDate <= endDate)
        //            .SelectMany(p => p.Medications, (p, pm) => new
        //            {
        //                Prescription = p,
        //                Medication = pm.Medication,
        //                Quantity = pm.Quantity,
        //                Repeats = pm.Repeats,
        //                Status = pm.Status,
        //                Doctor = p.Doctor
        //            })
        //            .Where(x => x.Status == DispenseStatus.Dispensed)
        //            .ToList();

        //        if (!prescriptions.Any())
        //        {
        //            document.Add(new Paragraph("No prescriptions dispensed for the selected date range.")
        //                .SetTextAlignment(TextAlignment.CENTER));
        //            document.Close();
        //            return memoryStream.ToArray();
        //        }

        //        // Grouping
        //        IEnumerable<IGrouping<string, dynamic>> groupedData = groupBy.ToLower() switch
        //        {
        //            "doctor" => prescriptions
        //                .GroupBy(x => $"{x.Doctor.DoctorName} {x.Doctor.DoctorSurname}")
        //                .OrderBy(g => g.Key),
        //            "medication" => prescriptions
        //                .GroupBy(x => x.Medication.MedicationName)
        //                .OrderBy(g => g.Key),
        //            _ => prescriptions.GroupBy(x => "All Prescriptions")
        //        };

        //        int grandTotal = 0;

        //        foreach (var group in groupedData)
        //        {
        //            document.Add(new Paragraph($"{groupBy.ToUpper()}: {group.Key}")
        //                .SetFontSize(14)
        //                .SetTextAlignment(TextAlignment.LEFT));

        //            var table = new Table(4).UseAllAvailableWidth();
        //            table.AddHeaderCell("Date");
        //            table.AddHeaderCell("Medication");
        //            table.AddHeaderCell("Qty");
        //            table.AddHeaderCell("Repeats");

        //            int subTotal = 0;

        //            foreach (var item in group)
        //            {
        //                table.AddCell(item.Prescription.ScriptDate.ToString("dd/MM/yyyy"));
        //                table.AddCell(item.Medication.MedicationName);
        //                table.AddCell(item.Quantity.ToString());
        //                table.AddCell(item.Repeats.ToString());

        //                subTotal += item.Quantity;
        //            }

        //            document.Add(table);
        //            document.Add(new Paragraph($"Sub-total: {subTotal}"));
        //            document.Add(new Paragraph(" ")); // spacing

        //            grandTotal += subTotal;
        //        }

        //        document.Add(new LineSeparator(new SolidLine()));
        //        document.Add(new Paragraph($"GRAND TOTAL: {grandTotal}").SetFontSize(14));

        //        document.Close();
        //        return memoryStream.ToArray();
        //    }
        //}


        //public byte[] GeneratePharmacistReport(
        //    string pharmacistId,
        //    DateTime startDate,
        //    DateTime endDate,
        //    string groupBy = "patient")
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        var writer = new PdfWriter(memoryStream);
        //        var pdf = new PdfDocument(writer);
        //        var document = new Document(pdf);

        //        // Title
        //        document.Add(new Paragraph("PRESCRIPTIONS DISPENSED")
        //            .SetFontSize(16)
        //            .SetTextAlignment(TextAlignment.CENTER));
        //        document.Add(new Paragraph($"Pharmacist ID: {pharmacistId}")
        //            .SetTextAlignment(TextAlignment.CENTER));
        //        document.Add(new Paragraph($"{startDate:dd/MM/yyyy} – {endDate:dd/MM/yyyy}")
        //            .SetTextAlignment(TextAlignment.CENTER));
        //        document.Add(new LineSeparator(new SolidLine()));
        //        document.Add(new Paragraph(" ")); // spacing

        //        // Fetch prescriptions
        //        var prescriptions = _context.Prescriptions
        //            .Where(p => p.PharmacistId == pharmacistId
        //                        && p.ScriptDate >= startDate
        //                        && p.ScriptDate <= endDate)
        //            .SelectMany(p => p.Medications, (p, pm) => new
        //            {
        //                Prescription = p,
        //                Medication = pm.Medication,
        //                Quantity = pm.Quantity,
        //                Instruction = pm.Instruction,
        //                Status = pm.Status,
        //                Customer = p.Customer
        //            })
        //            .Where(x => x.Status == DispenseStatus.Dispensed)
        //            .ToList();

        //        if (!prescriptions.Any())
        //        {
        //            document.Add(new Paragraph("No medications dispensed for the selected date range.")
        //                .SetTextAlignment(TextAlignment.CENTER));
        //            document.Close();
        //            return memoryStream.ToArray();
        //        }

        //        // Grouping
        //        IEnumerable<IGrouping<string, dynamic>> groupedData = groupBy.ToLower() switch
        //        {
        //            "patient" => prescriptions.GroupBy(x => $"{x.Customer.Name} {x.Customer.Surname}").OrderBy(g => g.Key),
        //            "medication" => prescriptions.GroupBy(x => x.Medication.MedicationName).OrderBy(g => g.Key),
        //            "schedule" => prescriptions.GroupBy(x => x.Medication.Schedule.ToString()).OrderBy(g => g.Key),
        //            _ => prescriptions.GroupBy(x => "All Prescriptions")
        //        };

        //        int grandTotal = 0;

        //        foreach (var group in groupedData)
        //        {
        //            document.Add(new Paragraph($"{groupBy.ToUpper()}: {group.Key}")
        //                .SetFontSize(14)
        //                .SetTextAlignment(TextAlignment.LEFT));

        //            var table = new Table(4).UseAllAvailableWidth();
        //            table.AddHeaderCell("Date");
        //            table.AddHeaderCell("Medication");
        //            table.AddHeaderCell("Qty");
        //            table.AddHeaderCell("Instructions");

        //            int subTotal = 0;

        //            foreach (var item in group)
        //            {
        //                table.AddCell(item.Prescription.ScriptDate.ToString("dd/MM/yyyy"));
        //                table.AddCell(item.Medication.MedicationName);
        //                table.AddCell(item.Quantity.ToString());
        //                table.AddCell(item.Instruction ?? "-");

        //                subTotal += item.Quantity;
        //            }

        //            document.Add(table);
        //            document.Add(new Paragraph($"Sub-total: {subTotal}"));
        //            document.Add(new Paragraph(" ")); // spacing

        //            grandTotal += subTotal;
        //        }

        //        document.Add(new LineSeparator(new SolidLine()));
        //        document.Add(new Paragraph($"GRAND TOTAL: {grandTotal}").SetFontSize(14));

        //        document.Close();
        //        return memoryStream.ToArray();
        //    }
        //}


        public byte[] GenerateManagerReport(List<Medication> medications, string groupBy)
        {
            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);

                // Create fonts - standard fonts don't take size parameters
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var italicFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

                // Add cover page
                AddCoverPage(document, boldFont, normalFont, italicFont);

                // Start new page for the actual report
                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                // Add header and footer to subsequent pages
                AddHeaderFooter(pdf, document, boldFont, normalFont);

                // Title
                document.Add(new Paragraph($"MEDICATION STOCK REPORT - GROUPED BY {groupBy.ToUpper()}")
                    .SetFont(boldFont)
                    .SetFontSize(18)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                document.Add(new LineSeparator(new SolidLine())
                    .SetMarginBottom(15));


                // Determine grouping logic
                IEnumerable<IGrouping<string, Medication>> groupedMeds = groupBy.ToLower() switch
                {
                    "supplier" => medications.GroupBy(m => m.Supplier?.SupplierName ?? "No Supplier").OrderBy(g => g.Key),
                    "dosageform" => medications.GroupBy(m => m.DosageForm?.DosageFormName ?? "No Dosage Form").OrderBy(g => g.Key),
                    "schedule" => medications.GroupBy(m => m.Schedule.ToString()).OrderBy(g => g.Key),
                    _ => medications.GroupBy(m => "All Medications")
                };

                decimal grandTotal = 0;

                foreach (var group in groupedMeds)
                {
                    // Group Header
                    document.Add(new Paragraph(group.Key.ToUpper())
                        .SetFont(boldFont)
                        .SetFontSize(14)
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetMarginBottom(10));

                    // Table
                    var table = new Table(6).UseAllAvailableWidth();

                    // Table headers
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Medication").SetFont(boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Dosage Form").SetFont(boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Schedule").SetFont(boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Stock On Hand").SetFont(boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Reorder Level").SetFont(boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Price").SetFont(boldFont)));

                    decimal groupSubtotal = 0;

                    foreach (var med in group)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(med.MedicationName)));
                        table.AddCell(new Cell().Add(new Paragraph(med.DosageForm?.DosageFormName ?? "N/A")));
                        table.AddCell(new Cell().Add(new Paragraph(med.Schedule.ToString())));
                        table.AddCell(new Cell().Add(new Paragraph(med.Quantity.ToString())));
                        table.AddCell(new Cell().Add(new Paragraph(med.ReOrderLevel.ToString())));
                        table.AddCell(new Cell().Add(new Paragraph(med.Price.ToString("C"))));

                        groupSubtotal += med.Price;
                    }

                    document.Add(table);

                    // Add group subtotal
                    document.Add(new Paragraph($"Sub-total for {group.Key}: {groupSubtotal.ToString("C")}")
                        .SetFont(boldFont)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetMarginTop(10)
                        .SetMarginBottom(20));

                    grandTotal += groupSubtotal;

                    // Check if we need a page break
                    if (document.GetRenderer().GetCurrentArea().GetBBox().GetHeight() < 150)
                    {
                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    }
                }

                // Add grand total
                document.Add(new Paragraph($"GRAND TOTAL: {grandTotal.ToString("C")}")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMarginTop(20));

                // Add report generation date
                document.Add(new Paragraph($"Report generated on: {DateTime.Now.ToString("dd MMMM yyyy 'at' HH:mm")}")
                    .SetFont(normalFont)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(30));

                document.Close();
                return memoryStream.ToArray();
            }
        }

        private void AddCoverPage(Document document, PdfFont boldFont, PdfFont normalFont, PdfFont italicFont)
        {
            // Add hospital image (you would replace this with your actual image path)
            try
            {
                var imagePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "HospitalCover.png");
                if (File.Exists(imagePath))
                {
                    ImageData imageData = ImageDataFactory.Create(imagePath);
                    Image image = new Image(imageData);
                    image.SetAutoScale(true);
                    image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                }
            }
            catch
            {
                // If image not found, continue without it
                document.Add(new Paragraph("[Hospital Image Placeholder]")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetFont(italicFont));
            }

            // Pharmacy details
            document.Add(new Paragraph("iBHAYI PHARMACY")
                .SetFont(boldFont)
                .SetFontSize(24)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(30)
                .SetMarginBottom(10));

            document.Add(new Paragraph("Professional Pharmaceutical Services")
                .SetFont(normalFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(30));

            document.Add(new LineSeparator(new SolidLine())
                .SetMarginBottom(30));


            document.Add(new Paragraph("MEDICATION STOCK REPORT")
                .SetFont(boldFont)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            
        }

        private void AddHeaderFooter(PdfDocument pdf, Document document, PdfFont boldFont, PdfFont normalFont)
        {
            int numberOfPages = pdf.GetNumberOfPages();
            for (int i = 1; i <= numberOfPages; i++)
            {
                PdfPage page = pdf.GetPage(i);

                // Skip header/footer on cover page
                if (i == 1) continue;

                // Header
                Canvas headerCanvas = new Canvas(page, page.GetPageSize());
                headerCanvas.Add(new Paragraph("iBHAYI PHARMACY - MEDICATION STOCK REPORT")
                    .SetFont(boldFont)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(20));
                headerCanvas.Add(new LineSeparator(new SolidLine(0.5f))
                    .SetMarginTop(5)
                    .SetMarginBottom(5));
                headerCanvas.Close();

                // Footer
                Canvas footerCanvas = new Canvas(page,
                    new Rectangle(page.GetPageSize().GetLeft(),
                                 page.GetPageSize().GetBottom(),
                                 page.GetPageSize().GetWidth(),
                                 30));
                footerCanvas.Add(new LineSeparator(new SolidLine(0.5f))
                    .SetMarginBottom(5));
                footerCanvas.Add(new Paragraph($"Page {i} of {numberOfPages} | Confidential")
                    .SetFont(normalFont)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER));
                footerCanvas.Add(new Paragraph($"Generated on {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}")
                    .SetFont(normalFont)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER));
                footerCanvas.Close();
            }
        }

    }
}