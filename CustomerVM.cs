using System.ComponentModel.DataAnnotations;

namespace PharmacyManager.Models.ViewModels
{
    public class CustomerVM
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Address { get; set; }
        public string SouthAfricanID { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password do not match.")]
        public string? ConfirmPassword { get; set; }
        public List<int>? allergyIds { get; set; } = new();
    }

    public class Step1VM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password), Required]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class Step2VM
    {
        [Required]
        public string Address { get; set; }

        [Required]
        public string SouthAfricanID { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
    }


}
