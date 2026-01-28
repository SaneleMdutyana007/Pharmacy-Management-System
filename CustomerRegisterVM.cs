using System.ComponentModel.DataAnnotations;

namespace PharmacyManager.Models.ViewModels
{
    public class CustomerRegisterVM
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage ="Password do not match.")]
        public required string ConfirmPassword { get; set; }
    }
}
