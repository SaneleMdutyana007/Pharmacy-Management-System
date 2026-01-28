using System.ComponentModel.DataAnnotations;

namespace PharmacyManager.Models.ViewModels
{
    public class StockAdjustmentVM
    {
        [Required(ErrorMessage = "Medication ID is required")]
        public int MedicationId { get; set; }

        [Required(ErrorMessage = "Adjustment type is required")]
        [RegularExpression("add|remove|set", ErrorMessage = "Adjustment type must be 'add', 'remove', or 'set'")]
        public string AdjustmentType { get; set; } // "add", "remove", "set"

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        // These are for display purposes, not required for form submission
        public string MedicationName { get; set; }
        public int CurrentQuantity { get; set; }
    }
}