using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Repositories;
using PharmacyManager.Repository;

namespace PharmacyManager.Controllers
{
    public class MedicationsController : Controller
    {
        private readonly IMedicationRepository _medicationRepo;
        private readonly IDosageFormRepository _dosageRepo;
        private readonly IActiveIngredientRepository _ingredientsRepo;
        private readonly ISupplierRepository _supplierRepo;

        public MedicationsController(
            IMedicationRepository medicationRepo,
            IActiveIngredientRepository ingredientsRepo,
            IDosageFormRepository dosageRepo,
            ISupplierRepository supplierRepo)
        {
            _medicationRepo = medicationRepo;
            _dosageRepo = dosageRepo;
            _supplierRepo = supplierRepo;
            _ingredientsRepo = ingredientsRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5;

            var medications = await _medicationRepo.GetAllMedications();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                medications = medications.Where(m =>
                    (m.MedicationName != null && m.MedicationName.Contains(searchString)) ||
                    m.Schedule.ToString().Contains(searchString) ||
                    (m.DosageForm != null && m.DosageForm.DosageFormName != null && m.DosageForm.DosageFormName.Contains(searchString)) ||
                    (m.Supplier != null && m.Supplier.SupplierName != null && m.Supplier.SupplierName.Contains(searchString))
                ).ToList();
            }

            var count = medications.Count();
            var items = medications
                .OrderBy(m => m.MedicationName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            ViewBag.SearchString = searchString;

            // Load dropdown data
            await ReLoadData();

            return View(items);
        }

        public async Task<ActionResult> Details(int id)
        {
            var medication = await _medicationRepo.GetMedicationByIdAsync(id);
            if (medication == null)
            {
                TempData["ErrorMessage"] = "Medication not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(medication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MedicationCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                await ReLoadData();

                // Get the current medications for the Index view
                var medications = await _medicationRepo.GetAllMedications();
                var count = medications.Count();
                var items = medications
                    .OrderBy(m => m.MedicationName)
                    .Take(8) // First page
                    .ToList();

                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = (int)Math.Ceiling(count / 8.0);

                // Store the model to repopulate the form
                ViewBag.CreateModel = model;

                // Let the view handle displaying validation errors
                return View("Index", items);
            }

            try
            {
                // Convert to MedicationVM for repository
                var medicationVM = new MedicationVM
                {
                    MedicationName = model.MedicationName ?? string.Empty,
                    Schedule = model.Schedule,
                    DosageId = model.DosageFormId,
                    SalesPrice = model.Price,
                    SupplierId = model.SupplierId,
                    QuantityOnHand = model.Quantity,
                    ReOrderLevel = model.ReOrderLevel,
                    Ingredients = new List<IngredientWithStrengthVM>()
                };

                // Add ingredients from the model
                if (model.Ingredients != null)
                {
                    foreach (var ingredient in model.Ingredients)
                    {
                        medicationVM.Ingredients.Add(new IngredientWithStrengthVM
                        {
                            ActiveIngredientId = ingredient.ActiveIngredientId,
                            Strength = ingredient.Strength ?? string.Empty
                        });
                    }
                }

                var result = await _medicationRepo.CreateAsync(medicationVM);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Medication created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create medication. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating medication: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MedicationEditVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Create updated medication object
                var updatedMedication = new Medication
                {
                    MedicationId = model.MedicationId,
                    MedicationName = model.MedicationName ?? string.Empty,
                    Schedule = model.Schedule,
                    DosageFormId = model.DosageFormId,
                    Price = model.Price,
                    SupplierId = model.SupplierId,
                    Quantity = model.Quantity,
                    ReOrderLevel = model.ReOrderLevel,
                    MedicationIngredients = new List<MedicationActiveIngredient>()
                };

                // Add ingredients from the model
                if (model.Ingredients != null)
                {
                    foreach (var ingredient in model.Ingredients)
                    {
                        updatedMedication.MedicationIngredients.Add(new MedicationActiveIngredient
                        {
                            ActiveIngredientId = ingredient.ActiveIngredientId,
                            Strength = ingredient.Strength ?? string.Empty,
                            MedicationId = model.MedicationId
                        });
                    }
                }

                var result = await _medicationRepo.UpdateAsync(updatedMedication);
                if (result)
                {
                    TempData["SuccessMessage"] = "Medication updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update medication. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating medication: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _medicationRepo.DeleteMedicationAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Medication deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete medication. It may be in use or not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting medication: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetMedication(int id)
        {
            var medication = await _medicationRepo.GetMedicationByIdAsync(id);
            if (medication == null)
            {
                return Json(new { success = false, message = "Medication not found." });
            }

            var medicationData = new
            {
                success = true,
                medicationId = medication.MedicationId,
                medicationName = medication.MedicationName,
                schedule = medication.Schedule,
                dosageFormId = medication.DosageFormId,
                price = medication.Price,
                supplierId = medication.SupplierId,
                quantity = medication.Quantity,
                reOrderLevel = medication.ReOrderLevel
            };

            return Json(medicationData);
        }

        [HttpGet]
        public async Task<JsonResult> GetMedicationWithIngredients(int id)
        {
            var medication = await _medicationRepo.GetMedicationByIdAsync(id);
            if (medication == null)
            {
                return Json(new { success = false, message = "Medication not found." });
            }

            // Fix the null-coalescing operator issue
            List<object> ingredients;
            if (medication.MedicationIngredients != null && medication.MedicationIngredients.Any())
            {
                ingredients = medication.MedicationIngredients.Select(ai => new
                {
                    activeIngredientId = ai.ActiveIngredientId,
                    ingredientName = ai.ActiveIngredient?.IngredientName ?? "Unknown",
                    strength = ai.Strength ?? string.Empty
                }).ToList<object>();
            }
            else
            {
                ingredients = new List<object>();
            }

            var medicationData = new
            {
                success = true,
                medicationId = medication.MedicationId,
                medicationName = medication.MedicationName,
                schedule = medication.Schedule,
                dosageFormId = medication.DosageFormId,
                price = medication.Price,
                supplierId = medication.SupplierId,
                quantity = medication.Quantity,
                reOrderLevel = medication.ReOrderLevel,
                ingredients = ingredients
            };

            return Json(medicationData);
        }

        private async Task ReLoadData()
        {
            ViewBag.ActiveIngredients = new SelectList(await _ingredientsRepo.GetAllActiveIngredients(), "ActiveIngredientId", "IngredientName");
            ViewBag.DosageForms = new SelectList(await _dosageRepo.GetAllDosageForms(), "DosageFormId", "DosageFormName");
            ViewBag.Suppliers = new SelectList(await _supplierRepo.GetAllSuppliers(), "SupplierId", "SupplierName");
        }
    }

}
