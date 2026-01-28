namespace PharmacyManager.Models.ViewModels
{
    public class CreateUserVM
    {
        public string Role { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string ContactNumber { get; set; }
        public string SouthAfricanID { get; set; } = string.Empty;
        public string Email { get; set; }
        public string? HealthCouncilNumber { get; set; }
    }
}
