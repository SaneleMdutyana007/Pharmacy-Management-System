using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManager.Models
{
    public class Medication
    {
        public int MedicationId { get; set; }
        public string MedicationName { get; set; }
        public int Schedule { get; set; }
        public int DosageFormId { get; set; }
        public DosageForm DosageForm { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        [Precision(8, 2)]

        public int Price { get; set; }
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        public int Quantity { get; set; }
        public int ReOrderLevel { get; set; }

      


        public ICollection<MedicationActiveIngredient> MedicationIngredients { get; set; } = new List<MedicationActiveIngredient>();
        public ICollection<PrescriptionMedication> Prescriptions { get; set; } = new List<PrescriptionMedication>();
    }
}
