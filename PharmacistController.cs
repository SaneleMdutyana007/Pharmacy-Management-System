using Microsoft.AspNetCore.Mvc;

namespace PharmacyManager.Controllers
{
    public class PharmacistController : BaseController
    {
        public IActionResult PharmacistPage()
        {
            return View();
        }

        public IActionResult LoadPrescription()
        {
            return View();
        }

        public IActionResult DispensePrescription()
        {
            return View();
        }
    }
}
