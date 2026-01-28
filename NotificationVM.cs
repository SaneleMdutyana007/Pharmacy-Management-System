namespace PharmacyManager.Models.ViewModels
{
    public class NotificationVM
    {
        public int Id { get; set; }
        public string Type { get; set; } // stock_low, doctor_added, etc.
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
        public string EntityType { get; set; } // Medication, Doctor, Supplier, Order
        public int? EntityId { get; set; }
    }
}
