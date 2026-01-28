using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManager.Data;
using PharmacyManager.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PharmacyManager.Controllers
{
    public class DosageFormsController : BaseController
    {
        private readonly PharmacyDbContext _context;

        public DosageFormsController(PharmacyDbContext context)
        {
            _context = context;
        }

        // GET: DosageForms
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 8;

            var dosageForms = from d in _context.DosageForms
                              select d;

            if (!string.IsNullOrEmpty(searchString))
            {
                dosageForms = dosageForms.Where(d => d.DosageFormName.Contains(searchString));
            }

            var count = await dosageForms.CountAsync();
            var items = await dosageForms
                .OrderBy(d => d.DosageFormName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(count / (double)pageSize);
            ViewBag.SearchString = searchString;

            return View(items);
        }

        // POST: DosageForms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DosageFormName")] DosageForm dosageForm)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicates
                if (await _context.DosageForms.AnyAsync(d => d.DosageFormName.ToLower() == dosageForm.DosageFormName.ToLower()))
                {
                    TempData["ErrorMessage"] = "A dosage form with this name already exists.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Add(dosageForm);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Dosage form added successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Error adding dosage form. Please check your input.";
            return RedirectToAction(nameof(Index));
        }

        // POST: DosageForms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DosageFormId,DosageFormName")] DosageForm dosageForm)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Error updating dosage form. Please check your input.";
                return RedirectToAction(nameof(Index));
            }

            // Check for duplicates (excluding current item)
            bool duplicateExists = await _context.DosageForms
                .AnyAsync(d => d.DosageFormName.ToLower() == dosageForm.DosageFormName.ToLower() &&
                              d.DosageFormId != dosageForm.DosageFormId);

            if (duplicateExists)
            {
                TempData["ErrorMessage"] = "A dosage form with this name already exists.";
                return RedirectToAction(nameof(Index));
            }

            // Find and update the existing dosage form
            var existingDosageForm = await _context.DosageForms
                .FirstOrDefaultAsync(d => d.DosageFormId == dosageForm.DosageFormId);

            if (existingDosageForm == null)
            {
                TempData["ErrorMessage"] = "Dosage form not found.";
                return RedirectToAction(nameof(Index));
            }

            // Update the properties
            existingDosageForm.DosageFormName = dosageForm.DosageFormName.Trim();

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Dosage form updated successfully!";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Error saving changes to database. Please try again.";
                // Log the exception here if you have logging configured
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: DosageForms/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dosageForm = await _context.DosageForms.FindAsync(id);
            if (dosageForm != null)
            {
                try
                {
                    _context.DosageForms.Remove(dosageForm);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Dosage form deleted successfully!";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Cannot delete this dosage form because it is being used by one or more medications.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dosage form not found!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DosageFormExists(int id)
        {
            return _context.DosageForms.Any(e => e.DosageFormId == id);
        }
    }
}