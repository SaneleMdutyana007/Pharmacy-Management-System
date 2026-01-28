using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManager.Areas.Identity.Data;
using PharmacyManager.Data;
using PharmacyManager.Services;

namespace PharmacyManager.Controllers
{
    public class ReportsController : BaseController
    {
        private readonly PharmacyDbContext _context;
        private readonly PDFService _pdfService;
        private UserManager<UserModel> _userManager;
        public ReportsController(PharmacyDbContext context, PDFService pdfService, UserManager<UserModel> userManager)
        {
            _context = context;
            _pdfService = pdfService;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var user = _userManager.GetUserAsync(User).Result;
            return View(user);
        }

        [HttpPost]
        public IActionResult CustomerReport()
        {
            var user = User.Identity.Name;
            return File(user, "application/pdf", $"{user}");
        }

        [HttpPost]
        public IActionResult ManagerReport(string groupBy)
        {
            var medications = _context.Medications
                                      .Include(d => d.DosageForm)
                                      .Include(a => a.MedicationIngredients)
                                      .ThenInclude(a => a.ActiveIngredient)
                                      .Include(s => s.Supplier)
                                      .ToList();
            var pdfBytes = _pdfService.GenerateManagerReport(medications, groupBy);

            return File(pdfBytes, "application/pdf", "MedicationStockReport.pdf");
        }

        [HttpPost]
        public IActionResult PharmacistReport()
        {
            var user = User.Identity.Name;
            return File(user, "application/pdf", $"{user}");
        }
    }
}