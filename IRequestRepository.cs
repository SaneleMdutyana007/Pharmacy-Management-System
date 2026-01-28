using Microsoft.EntityFrameworkCore;
using PharmacyManager.Data;
using PharmacyManager.Data.DTO;
using PharmacyManager.Models;

namespace PharmacyManager.Repositories
{
    public interface IRequestRepository
    {
        Task<FileUploadResult> UploadPrescription(IFormFile pdfFile);
        Task<bool> CreateRequest(string filePath, string fileName, string customerId);
        Task<PrescriptionRequest> GetRequestById(int id);
        Task<IEnumerable<PrescriptionRequest>> GetAllRequests();
        Task<FileStream>GetPDF(int Id);
    }

    public class RequestRepository : IRequestRepository
    {
        private readonly IWebHostEnvironment _env;
        private readonly PharmacyDbContext _db;
        public RequestRepository(PharmacyDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<FileUploadResult> UploadPrescription(IFormFile pdfFile)
        {
            try
            {
                if (pdfFile == null || pdfFile.Length == 0)
                    return new FileUploadResult { Success = false, ErrorMessage = "No file uploaded." };

                if (!pdfFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                    return new FileUploadResult { Success = false, ErrorMessage = "Invalid file type. Please upload a PDF file." };

                // Store outside wwwroot
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads", "Scripts");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(pdfFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                var relativePath = Path.GetRelativePath(_env.ContentRootPath, filePath);

                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await pdfFile.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                // Save file for now (not linked to prescription yet)
                await File.WriteAllBytesAsync(filePath, fileBytes);

                return new FileUploadResult
                {
                    Success = true,
                    FileBytes = fileBytes,
                    FilePath = relativePath, // FULL secure path
                    FileName = uniqueFileName
                };
            }
            catch (Exception ex)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> CreateRequest(string filePath, string fileName, string customerId)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileName))
                return false;

            if (!File.Exists(filePath))
                return false;

            try
            {
                var fileBytes = await File.ReadAllBytesAsync(filePath);

                var request = new PrescriptionRequest
                {
                    FileName = fileName,
                    FilePath = filePath,
                    CustomerId = customerId,
                    Status = RequestStatus.Pending,
                    UploadedAt = DateTime.UtcNow
                };

                await _db.PrescriptionRequests.AddAsync(request);
                var saved = await _db.SaveChangesAsync() > 0;

                //if (saved)
                //{
                //    // Delete the uploaded file after saving to DB to avoid clutter
                //    File.Delete(filePath);
                //}

                return saved;
            }
            catch (Exception ex)
            {
                
                return false;
            }
        }

        public async Task<PrescriptionRequest> GetRequestById(int id)
        {
            return await _db.PrescriptionRequests.AsNoTracking().Include(c => c.Customer).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<PrescriptionRequest>> GetAllRequests()
        {
            return await _db.PrescriptionRequests.AsNoTracking().ToListAsync();
        }

        public async Task<FileStream> GetPDF(int id)
        {
            var request = await GetRequestById(id);
            if (request == null || string.IsNullOrEmpty(request.FilePath))
            {
                return null;
            }

            // Reconstruct the full file path from the relative path stored in the database.
            var fullPath = Path.Combine(_env.ContentRootPath, request.FilePath);

            if (!File.Exists(fullPath))
            {
                return null;
            }

            // Read the file bytes and serve it.
            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        }
    }
}