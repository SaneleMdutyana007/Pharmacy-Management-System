using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PharmacyManager.Models.ViewModels
{
    public class MedicationVM
    {
        [DisplayName("Medication Name")]
        public string MedicationName { get; set; }

        [DisplayName("Dosage Form")]
        public int DosageId { get; set; }

        [DisplayName("Select Supplier")]
        public int SupplierId { get; set; }

        public List<IngredientWithStrengthVM> Ingredients { get; set; } = new List<IngredientWithStrengthVM>();

        public int Schedule { get; set; }

        [DisplayName("Set Price")]
        public int SalesPrice { get; set; }

        [DisplayName("reOrder Level")]
        public int ReOrderLevel { get; set; } = 10;

        [DisplayName("Available Qty")]
        public int QuantityOnHand { get; set; }
    }
    public class MedicationCreateVM
    {
        [Required(ErrorMessage = "Medication name is required")]
        [StringLength(100, ErrorMessage = "Medication name cannot exceed 100 characters")]
        public string MedicationName { get; set; }

        [Required(ErrorMessage = "Schedule is required")]
        [Range(0, 7, ErrorMessage = "Schedule must be between 0 and 7")]
        public int Schedule { get; set; }

        [Required(ErrorMessage = "Dosage form is required")]
        public int DosageFormId { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 10000, ErrorMessage = "Price must be greater than 0")]
        public int Price { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Reorder level is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative")]
        public int ReOrderLevel { get; set; }

        public List<MedicationIngredientVM> Ingredients { get; set; } = new List<MedicationIngredientVM>();
    }
    public class MedicationEditVM
    {
        public int MedicationId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public int Schedule { get; set; }
        public int DosageFormId { get; set; }
        public int Price { get; set; }
        public int SupplierId { get; set; }
        public int Quantity { get; set; }
        public int ReOrderLevel { get; set; }
        public List<MedicationIngredientVM> Ingredients { get; set; } = new List<MedicationIngredientVM>();
    }

    public class MedicationIngredientVM
    {
        public int ActiveIngredientId { get; set; }
        public string Strength { get; set; } = string.Empty;
    }




}
