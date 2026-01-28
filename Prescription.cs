using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManager.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        public required string PatientId { get; set; }
        [ForeignKey("PatientId")]
        public virtual Customer Patient { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }

        [DataType(DataType.Date)]
        public DateTime ScriptDate { get; set; }

        public ICollection<PrescriptionMedication> Medications { get; set; } = new List<PrescriptionMedication>();
        public string Status { get; set; } = "";

        public decimal TotalPrice()
        {
            decimal totalPrice = 0;

            foreach (var med in Medications)
            {
                totalPrice += med.Medication.Price * med.Quantity;
            }
            return totalPrice;
        }

        //public int Repeats { get; set; } = 0;
    }
}
