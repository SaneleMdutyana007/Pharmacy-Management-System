namespace PharmacyManager.Data.DTO
{
    public class FileUploadResult
    {
        public bool Success { get; set; }
        public byte[] FileBytes { get; set; }
        public string FilePath { get; set; } // Relative path like /uploads/filename.pdf
        public string FileName { get; set; }
        public string ErrorMessage { get; set; } // Optional error message if upload fails
    }

}
