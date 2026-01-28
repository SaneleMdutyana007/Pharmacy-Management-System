using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PharmacyManager.Areas.Identity.Data
{
    public class UserModel : IdentityUser
    {
        [Required]
        [StringLength(20)]
        public string? Name { get; set; }
        [Required]
        [StringLength(20)]
        public string? Surname { get; set; }
        public string? ContactNumber { get; set; }

        public string? SouthAfricanID { get; set; }
        public string? UserType { get; set; }

        public DateTime? DateCreated { get; set; } = DateTime.Now;
        public string? Status { get; set; } = "Active";

        public string? ImagePath { get; set; }
        public byte[]? ImageData { get; set; }

    }
}
