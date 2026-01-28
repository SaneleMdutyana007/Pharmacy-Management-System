namespace PharmacyManager.Models
{
    public class PrescriptionRequest
    {
        public int Id { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public string? CustomerId { get; set; }
        public Customer Customer { get; set; }

        // Pharmacist review status
        public RequestStatus Status { get; set; } = RequestStatus.Pending; // Or enum: Pending, Approved, Rejected

        // Link to the actual created prescription
        public int? PrescriptionId { get; set; }
        public Prescription? Prescription { get; set; }
    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
