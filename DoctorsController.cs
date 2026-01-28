using Microsoft.AspNetCore.Mvc;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Repository;
using PharmacyManager.Services;
using System.Threading.Tasks;

namespace PharmacyManager.Controllers
{
    public class DoctorsController : BaseController
    {
        private readonly IDoctorRepository _repo;
        private readonly INotificationService _notificationService;

        public DoctorsController(IDoctorRepository repo, INotificationService notificationService)
        {
            _repo = repo;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 3;

            var doctors = await _repo.GetAllDoctors();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                doctors = doctors.Where(d =>
                    (d.DoctorName != null && d.DoctorName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (d.DoctorSurname != null && d.DoctorSurname.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (d.PracticeNumber != null && d.PracticeNumber.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (d.PhoneNumber != null && d.PhoneNumber.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (d.Email != null && d.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            var count = doctors.Count();
            var items = doctors
                .OrderBy(d => d.DoctorName)
                .ThenBy(d => d.DoctorSurname)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            ViewBag.SearchString = searchString;

            return View(items);
        }

        // POST: Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var doctorVM = new DoctorVM
                {
                    DoctorName = model.DoctorName ?? string.Empty,
                    DoctorSurname = model.DoctorSurname ?? string.Empty,
                    PracticeNumber = model.PracticeNumber ?? string.Empty,
                    PhoneNumber = model.PhoneNumber ?? string.Empty,
                    Email = model.Email ?? string.Empty
                };

                var result = await _repo.CreateAsync(doctorVM);
                if (result)
                {
                    await _notificationService.CreateNotificationAsync(
                       "doctor_added",
                       $"New doctor added: {model.DoctorName} {model.DoctorSurname}",
                       "Doctor"
                       
                       );
                   TempData["SuccessMessage"] = "Doctor created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create doctor. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating doctor: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Doctors/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DoctorEditVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var doctor = new Doctor
                {
                    DoctorId = model.DoctorId,
                    DoctorName = model.DoctorName ?? string.Empty,
                    DoctorSurname = model.DoctorSurname ?? string.Empty,
                    PracticeNumber = model.PracticeNumber ?? string.Empty,
                    PhoneNumber = model.PhoneNumber ?? string.Empty,
                    Email = model.Email ?? string.Empty
                };

                var result = await _repo.UpdateAsync(doctor);
                if (result)
                {
                    TempData["SuccessMessage"] = "Doctor updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update doctor. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating doctor: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Doctors/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _repo.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Doctor deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete doctor. It may be in use or not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting doctor: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Doctors/GetDoctor/{id} - For AJAX calls to populate edit modal
        [HttpGet]
        public async Task<JsonResult> GetDoctor(int id)
        {
            var doctor = await _repo.GetDoctorByIdAsync(id);
            if (doctor == null)
            {
                return Json(new { success = false, message = "Doctor not found." });
            }

            var doctorData = new
            {
                success = true,
                doctorId = doctor.DoctorId,
                doctorName = doctor.DoctorName,
                doctorSurname = doctor.DoctorSurname,
                practiceNumber = doctor.PracticeNumber,
                phoneNumber = doctor.PhoneNumber,
                email = doctor.Email
            };

            return Json(doctorData);
        }

        // GET: Doctors/Details/{id} - For direct navigation (optional)
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _repo.GetDoctorByIdAsync(id);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }
    }
}