using System.ComponentModel.DataAnnotations;

namespace PharmacyManager.Models.ViewModels
{
    public class PrescriptionVM
    {
        public string PatientId { get; set; }

        public int DoctorId { get; set; }

        [DataType(DataType.Date)]
        public DateTime ScriptDate { get; set; }

        public ICollection<PrescriptionMedicationVM> Medications { get; set; } = new List<PrescriptionMedicationVM>();
        public string Status { get; set; } = "";
    }

    public class PrescriptionMedicationVM
    {
        public int MedicationId { get; set; }

        public int Quantity { get; set; }

        public string Instruction { get; set; } = "";
    }

}
