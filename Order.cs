using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManager.Models
{
    public class Order
    {
        [Key]
        public int OrdersId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderDate { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ReceiveDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public int TotalQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        public ICollection<OrderMedication> OrderMedications { get; set; }
    }

    public class OrderMedication
    {
        [Key]
        public int OrderMedicationId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [Required]
        public int MedicationId { get; set; }

        [ForeignKey("MedicationId")]
        public Medication? Medication { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}