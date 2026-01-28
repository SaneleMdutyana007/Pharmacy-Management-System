using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManager.Data;
using PharmacyManager.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PharmacyManager.Controllers
{
    public class ActiveIngredientsController : Controller
    {
        private readonly PharmacyDbContext _context;

        public ActiveIngredientsController(PharmacyDbContext context)
        {
            _context = context;
        }

        // GET: ActiveIngredients
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 8;

            var ingredients = from i in _context.ActiveIngredients
                              select i;

            if (!string.IsNullOrEmpty(searchString))
            {
                ingredients = ingredients.Where(i => i.IngredientName.Contains(searchString));
            }

            var count = await ingredients.CountAsync();
            var items = await ingredients
                .OrderBy(i => i.IngredientName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(count / (double)pageSize);
            ViewBag.SearchString = searchString;

            return View(items);
        }

        // POST: ActiveIngredients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IngredientName")] ActiveIngredient activeIngredient)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicates
                if (await _context.ActiveIngredients.AnyAsync(i => i.IngredientName.ToLower() == activeIngredient.IngredientName.ToLower()))
                {
                    TempData["ErrorMessage"] = "An active ingredient with this name already exists.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Add(activeIngredient);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Active ingredient added successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Error adding active ingredient. Please check your input.";
            return RedirectToAction(nameof(Index));
        }

        // POST: ActiveIngredients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ActiveIngredientId,IngredientName")] ActiveIngredient activeIngredient)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Error updating active ingredient. Please check your input.";
                return RedirectToAction(nameof(Index));
            }

            // Check for duplicates (excluding current item)
            bool duplicateExists = await _context.ActiveIngredients
                .AnyAsync(i => i.IngredientName.ToLower() == activeIngredient.IngredientName.ToLower() &&
                              i.ActiveIngredientId != activeIngredient.ActiveIngredientId);

            if (duplicateExists)
            {
                TempData["ErrorMessage"] = "An active ingredient with this name already exists.";
                return RedirectToAction(nameof(Index));
            }

            // Find and update the existing ingredient
            var existingIngredient = await _context.ActiveIngredients
                .FirstOrDefaultAsync(i => i.ActiveIngredientId == activeIngredient.ActiveIngredientId);

            if (existingIngredient == null)
            {
                TempData["ErrorMessage"] = "Active ingredient not found.";
                return RedirectToAction(nameof(Index));
            }

            // Update the properties
            existingIngredient.IngredientName = activeIngredient.IngredientName.Trim();

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Active ingredient updated successfully!";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Error saving changes to database. Please try again.";
                // Log the exception here if you have logging configured
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ActiveIngredients/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var activeIngredient = await _context.ActiveIngredients.FindAsync(id);
            if (activeIngredient != null)
            {
                try
                {
                    _context.ActiveIngredients.Remove(activeIngredient);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Active ingredient deleted successfully!";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Cannot delete this active ingredient because it is being used by one or more medications.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Active ingredient not found!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ActiveIngredientExists(int id)
        {
            return _context.ActiveIngredients.Any(e => e.ActiveIngredientId == id);
        }
    }
}