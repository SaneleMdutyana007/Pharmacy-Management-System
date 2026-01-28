using System.ComponentModel.DataAnnotations;

namespace PharmacyManager.Models
{

    public class Stock
    {
        public int StockId { get; set; }
        public int MedicationId { get; set; }
        public Medication Medications { get; set; }
        public Supplier supplier { get; set; }

        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Status { get; set; }


    }
}


