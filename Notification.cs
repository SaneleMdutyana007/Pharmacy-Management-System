using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PharmacyManager.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } // stock_low, doctor_added, supplier_added, medicine_added, order_placed, order_completed, order_pending

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        [StringLength(50)]
        public string? EntityType { get; set; } // Medication, Doctor, Supplier, Order

        // Separate foreign keys
        public int? MedicationId { get; set; }
        public int? DoctorId { get; set; }
        public int? SupplierId { get; set; }
        public int? OrderId { get; set; }
        // Navigation properties
        [ForeignKey("MedicationId")]
        public virtual Medication? Medication { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }

}
