namespace PharmacyManager.Models.ViewModels
{
        public class SupplierVM
        {
            public string SupplierName { get; set; }
            public string ContactPerson { get; set; }
            public string Email { get; set; }
        }
        public class SupplierCreateVM
        {
            public string SupplierName { get; set; } = string.Empty;
            public string ContactPerson { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class SupplierEditVM
        {
            public int SupplierId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public string ContactPerson { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

}
