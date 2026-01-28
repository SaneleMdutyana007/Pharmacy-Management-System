namespace PharmacyManager.Models.ViewModels
{
    public class StockVM
    {
      
            public int MedicationId { get; set; }
            public string MedicationName { get; set; }
            public int Schedule { get; set; }
            public string DosageForm { get; set; }
            public string SupplierName { get; set; }
            public int SupplierId { get; set; }
            public int Quantity { get; set; }
            public int ReOrderLevel { get; set; }
            public string Status { get; set; }
            public decimal Price { get; set; }
        
    }
}
