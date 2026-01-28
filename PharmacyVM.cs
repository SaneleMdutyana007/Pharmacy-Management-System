namespace PharmacyManager.Models.ViewModels
{
    public class PharmacyVM
    {
        public int Id { get; set; }
        public string PharmacyName { get; set; }
        public string HealthCouncilRegNum { get; set; }
        public string ResponsiblePharma { get; set; }
        public string PhysicalAddress { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string PharmacyURL { get; set; }
        public int VAT { get; set; }
    }
}
