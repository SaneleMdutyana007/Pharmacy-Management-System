using System.ComponentModel.DataAnnotations;

namespace PharmacyManager.Models
{
    public class MedicationActiveIngredient
    {
        [Key]
        public Guid Id { get; set; }

        public int MedicationId { get; set; }
        public Medication Medication { get; set; }

        public int ActiveIngredientId { get; set; }
        public ActiveIngredient ActiveIngredient { get; set; }

        public string Strength { get; set; }
    }
}
