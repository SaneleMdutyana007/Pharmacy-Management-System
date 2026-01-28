using PharmacyManager.Areas.Identity.Data;

namespace PharmacyManager.Models
{
    public class Pharmacist : UserModel
    {
        public string HealthCouncilNumber { get; set; }
        public bool FirstLogin { get; set; } = true;
    }
}
