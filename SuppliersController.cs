using Microsoft.AspNetCore.Mvc;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Repositories;
using PharmacyManager.Repository;
using PharmacyManager.Services;

namespace PharmacyManager.Controllers
{
    public class SuppliersController : BaseController
    {
        private readonly ISupplierRepository _repo;
        private readonly INotificationService _notificationService;

        public SuppliersController(ISupplierRepository repo, INotificationService notificationService)
        {
            _repo = repo;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 3;

            var suppliers = await _repo.GetAllSuppliers();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                suppliers = suppliers.Where(s =>
                   (s.SupplierName != null && s.SupplierName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                   (s.ContactPerson != null && s.ContactPerson.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                   (s.Email != null && s.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)) 
               ).ToList();
            }

            var count = suppliers.Count();
            var items = suppliers
                .OrderBy(s => s.SupplierName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            ViewBag.SearchString = searchString;

            return View(items);
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var supplierVM = new SupplierVM
                {
                    SupplierName = model.SupplierName ?? string.Empty,
                    ContactPerson = model.ContactPerson ?? string.Empty,
                    Email = model.Email ?? string.Empty,
                };

                var result = await _repo.CreateAsync(supplierVM);
                if (result != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        "supplier_added",
                        $"New supplier added: {model.SupplierName}",
                        "Supplier",
                        result.SupplierId
                    );

                    TempData["SuccessMessage"] = "Supplier created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create supplier. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating supplier: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Suppliers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SupplierEditVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var supplier = new Supplier
                {
                    SupplierId = model.SupplierId,
                    SupplierName = model.SupplierName ?? string.Empty,
                    ContactPerson = model.ContactPerson ?? string.Empty,
                    Email = model.Email ?? string.Empty,
                };

                var result = await _repo.UpdateAsync(supplier);
                if (result)
                {
                    TempData["SuccessMessage"] = "Supplier updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update supplier. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating supplier: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Suppliers/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _repo.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Supplier deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete supplier. It may be in use or not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting supplier: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Suppliers/GetSupplier/{id} - For AJAX calls to populate edit modal
        [HttpGet]
        public async Task<JsonResult> GetSupplier(int id)
        {
            var supplier = await _repo.GetSupplierByIdAsync(id);
            if (supplier == null)
            {
                return Json(new { success = false, message = "Supplier not found." });
            }

            var supplierData = new
            {
                success = true,
                supplierId = supplier.SupplierId,
                supplierName = supplier.SupplierName,
                contactPerson = supplier.ContactPerson,
                email = supplier.Email,
            };

            return Json(supplierData);
        }

        // GET: Suppliers/Details/{id} - For direct navigation (optional)
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _repo.GetSupplierByIdAsync(id);
            if (supplier == null)
            {
                TempData["ErrorMessage"] = "Supplier not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }
    }
}