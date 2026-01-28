using Microsoft.AspNetCore.Mvc;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Repository;

namespace PharmacyManager.Controllers
{
    public class PharmacyDetailsController : BaseController
    {
        private readonly IPharmacyRepository _pharmacyRepository;

        public PharmacyDetailsController(IPharmacyRepository pharmacyRepository)
        {
            _pharmacyRepository = pharmacyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var details = await _pharmacyRepository.GetPharmacyInfo();
                return View(details);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading pharmacy details: " + ex.Message;
                return View(new Pharmacy());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update()
        {
            try
            {
                var pharmacy = await _pharmacyRepository.GetPharmacyInfo();
                if (pharmacy == null || pharmacy.Id == 0)
                {
                    TempData["ErrorMessage"] = "No pharmacy details found. Please create pharmacy details first.";
                    return RedirectToAction("Create");
                }
                return View(pharmacy);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading pharmacy details: " + ex.Message;
                return RedirectToAction("Create");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Pharmacy model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the validation errors.";
                return View(model);
            }

            try
            {
                var success = await _pharmacyRepository.Update(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Pharmacy details updated successfully!";
                    return RedirectToAction("Manager", "Dashboard");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update pharmacy details. Please try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating pharmacy details: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Check if pharmacy details already exist
            if (await _pharmacyRepository.CheckIfDetailsExist())
            {
                TempData["ErrorMessage"] = "Pharmacy details already exist. You can update them from the dashboard.";
                return RedirectToAction("Manager", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PharmacyVM model)
        {
            // Check if details already exist (in case user navigated directly to URL)
            if (await _pharmacyRepository.CheckIfDetailsExist())
            {
                TempData["ErrorMessage"] = "Pharmacy details already exist. You can update them from the dashboard.";
                return RedirectToAction("Manager", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = "Please correct the following errors: " + string.Join(", ", errors);
                return View(model);
            }

            try
            {
                bool success = await _pharmacyRepository.Add(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Pharmacy details created successfully!";
                    return RedirectToAction("Manager", "Dashboard");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create pharmacy details. Please try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating pharmacy details: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPharmacyDetails()
        {
            try
            {
                var pharmacy = await _pharmacyRepository.GetPharmacyInfo();

                if (pharmacy != null && pharmacy.Id > 0)
                {
                    return Ok(new
                    {
                        id = pharmacy.Id,
                        pharmacyName = pharmacy.PharmacyName,
                        healthCouncilRegNum = pharmacy.HealthCouncilRegNum,
                        responsiblePharma = pharmacy.ResponsiblePharma,
                        physicalAddress = pharmacy.PhysicalAddress,
                        contact = pharmacy.Contact,
                        email = pharmacy.Email,
                        pharmacyURL = pharmacy.PharmacyURL,
                        vat = pharmacy.VAT
                    });
                }
                else
                {
                    return Ok(null);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error loading pharmacy details: " + ex.Message });
            }
        }
    }
}