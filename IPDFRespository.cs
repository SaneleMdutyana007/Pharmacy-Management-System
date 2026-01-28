using Microsoft.AspNetCore.Identity;
using PharmacyManager.Areas.Identity.Data;
using PharmacyManager.Data;
using PharmacyManager.Models;

namespace PharmacyManager.Repositories
{
    public interface IPDFRespository
    {
        Task<bool> UploadPdf(IFormFile pdf, UserModel user);
    }

    public class PDFRepository : IPDFRespository
    {
        private readonly IWebHostEnvironment _web;
        private readonly UserManager<UserModel> _userManager;
        private readonly PharmacyDbContext _context;

        public PDFRepository(IWebHostEnvironment web, UserManager<UserModel> userManager, PharmacyDbContext context)
        {
            _web = web;
            _userManager = userManager;
            _context = context;
        }
        public async Task<bool> UploadPdf(IFormFile pdf, UserModel user)
        {
            if (pdf == null || Path.GetExtension(pdf.FileName).ToLower() != ".pdf") return false;

            // Save to memory
            using var ms = new MemoryStream();
            await pdf.CopyToAsync(ms);
            var fileData = ms.ToArray();

            // Save to disk
            var docsFolder = Path.Combine(_web.WebRootPath, "docs");
            Directory.CreateDirectory(docsFolder);

            var uniqueName = $"{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(docsFolder, uniqueName);
            await File.WriteAllBytesAsync(filePath, fileData);

            var request = new PrescriptionRequest
            {
                FileName = pdf.FileName,
                FilePath = $"docs/{uniqueName}", // Relative path for Razor
                CustomerId = user.Id
            };

            _context.PrescriptionRequests.Add(request);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
