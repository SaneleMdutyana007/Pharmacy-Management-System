namespace PharmacyManager.Models
{
    public class PrescriptionMedication
    {
        public int Id { get; set; }

        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; }

        public int MedicationId { get; set; }
        public Medication Medication { get; set; }

        public int Quantity { get; set; }
        public string Instruction { get; set; }
    }
}
