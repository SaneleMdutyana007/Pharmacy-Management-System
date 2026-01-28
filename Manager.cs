using PharmacyManager.Areas.Identity.Data;

namespace PharmacyManager.Models
{
    public class Manager: UserModel
    {
        public bool FirstLogin { get; set; } = true;
    }
}
