using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Repositories;
using PharmacyManager.Repository;

namespace PharmacyManager.Controllers
{
    public class StockController : Controller
    {
        private readonly IMedicationRepository _medicationRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly IStockRepository _stockRepo;

        public StockController(
            IMedicationRepository medicationRepo,
            ISupplierRepository supplierRepo,
            IStockRepository stockRepo)
        {
            _medicationRepo = medicationRepo;
            _supplierRepo = supplierRepo;
            _stockRepo = stockRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string supplierFilter, string statusFilter, int page = 1)
        {
            int pageSize = 5;

            var medications = await _medicationRepo.GetAllMedications();

           
            // Apply supplier filter
            if (!string.IsNullOrEmpty(supplierFilter) && supplierFilter != "all")
            {
                medications = medications.Where(m =>
                    m.Supplier != null && m.Supplier.SupplierId.ToString() == supplierFilter
                ).ToList();
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                medications = statusFilter switch
                {
                    "critical" => medications.Where(m => m.Quantity < m.ReOrderLevel).ToList(),
                    "low" => medications.Where(m => m.Quantity >= m.ReOrderLevel && m.Quantity <= m.ReOrderLevel + 10).ToList(),
                    "adequate" => medications.Where(m => m.Quantity > m.ReOrderLevel + 10).ToList(),
                    _ => medications
                };
            }

            // Create stock view models
            var stockItems = medications.Select(m => new StockVM
            {
                MedicationId = m.MedicationId,
                MedicationName = m.MedicationName,
                Schedule = m.Schedule,
                DosageForm = m.DosageForm?.DosageFormName ?? "N/A",
                SupplierName = m.Supplier?.SupplierName ?? "N/A",
                SupplierId = m.SupplierId,
                Quantity = m.Quantity,
                ReOrderLevel = m.ReOrderLevel,
                Status = GetStockStatus(m.Quantity, m.ReOrderLevel),
                Price = m.Price
            }).ToList();

            var count = stockItems.Count();
            var items = stockItems
                .OrderBy(m => m.MedicationName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            ViewBag.SearchString = searchString;
            ViewBag.SupplierFilter = supplierFilter;
            ViewBag.StatusFilter = statusFilter;

            // Load dropdown data
            await LoadFilterData();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStock(StockAdjustmentVM model)
        {
            // Basic validation
            if (model.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "Quantity must be greater than 0.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(model.AdjustmentType) || !new[] { "add", "remove", "set" }.Contains(model.AdjustmentType))
            {
                TempData["ErrorMessage"] = "Please select a valid adjustment type.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var medication = await _medicationRepo.GetMedicationByIdAsync(model.MedicationId);
                if (medication == null)
                {
                    TempData["ErrorMessage"] = "Medication not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Additional business logic validation
                if (model.AdjustmentType == "remove" && medication.Quantity < model.Quantity)
                {
                    TempData["ErrorMessage"] = $"Cannot remove {model.Quantity} units. Only {medication.Quantity} units available.";
                    return RedirectToAction(nameof(Index));
                }

                bool result = false;
                switch (model.AdjustmentType)
                {
                    case "add":
                        result = await _stockRepo.AddStockAsync(model.MedicationId, model.Quantity);
                        break;
                    case "remove":
                        result = await _stockRepo.RemoveStockAsync(model.MedicationId, model.Quantity);
                        break;
                    case "set":
                        result = await _stockRepo.SetStockAsync(model.MedicationId, model.Quantity);
                        break;
                }

                if (result)
                {
                    TempData["SuccessMessage"] = $"Stock {model.AdjustmentType} operation completed successfully! Current quantity: {await GetCurrentQuantity(model.MedicationId)}";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to {model.AdjustmentType} stock. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adjusting stock: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetStockDetails(int id)
        {
            var medication = await _medicationRepo.GetMedicationByIdAsync(id);
            if (medication == null)
            {
                return Json(new { success = false, message = "Medication not found." });
            }

            var stockData = new
            {
                success = true,
                medicationId = medication.MedicationId,
                medicationName = medication.MedicationName,
                currentQuantity = medication.Quantity,
                reOrderLevel = medication.ReOrderLevel,
                status = GetStockStatus(medication.Quantity, medication.ReOrderLevel)
            };

            return Json(stockData);
        }

        private async Task<int> GetCurrentQuantity(int medicationId)
        {
            var medication = await _medicationRepo.GetMedicationByIdAsync(medicationId);
            return medication?.Quantity ?? 0;
        }

        private string GetStockStatus(int quantity, int reOrderLevel)
        {
            if (quantity < reOrderLevel)
                return "critical";
            else if (quantity >= reOrderLevel && quantity <= reOrderLevel + 10)
                return "low";
            else
                return "adequate";
        }

        private async Task LoadFilterData()
        {
            var suppliers = await _supplierRepo.GetAllSuppliers();
            ViewBag.Suppliers = new SelectList(suppliers, "SupplierId", "SupplierName");

            ViewBag.StatusTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "all", Text = "All Status" },
                new SelectListItem { Value = "critical", Text = "Critical" },
                new SelectListItem { Value = "low", Text = "Low" },
                new SelectListItem { Value = "adequate", Text = "Adequate" }
            };
        }
    }
}