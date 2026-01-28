using Microsoft.EntityFrameworkCore;
using PharmacyManager.Data;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Utilities;

namespace PharmacyManager.Repository
{
    public interface IMedicationRepository
    {
        Task<Medication> CreateAsync(MedicationVM model);
        Task<bool> UpdateAsync(Medication model);
        Task<bool> DeleteMedicationAsync(int Id);
        Task<IEnumerable<Medication>> GetAllMedications();
        Task<Medication?> GetMedicationByIdAsync(int id);
        Task UpdateMedicationAsync(Medication medication);

    }

    public class MedicationRepository : IMedicationRepository
    {
        private readonly IMapper _mapper;
        private readonly PharmacyDbContext _db;
        public MedicationRepository(IMapper mapper, PharmacyDbContext db)
        {
            _mapper = mapper;
            _db = db;
        }

        public async Task<Medication> CreateAsync(MedicationVM model)
        {
            if (model == null) return null!;

            var medication = new Medication
            {
                MedicationName = model.MedicationName ?? string.Empty,
                DosageFormId = model.DosageId,
                SupplierId = model.SupplierId,
                Schedule = model.Schedule,
                Price = model.SalesPrice,
                ReOrderLevel = model.ReOrderLevel,
                Quantity = model.QuantityOnHand
            };

            // Only add active ingredients if provided
            if (model.Ingredients != null && model.Ingredients.Any())
            {
                medication.MedicationIngredients = model.Ingredients.Select(i => new MedicationActiveIngredient
                {
                    ActiveIngredientId = i.ActiveIngredientId,
                    Strength = i.Strength ?? string.Empty
                }).ToList();
            }

            _db.Medications.Add(medication);
            await _db.SaveChangesAsync();
            return medication;
        }

        public async Task<bool> DeleteMedicationAsync(int Id)
        {
            var medication = await _db.Medications
                                      .Include(m => m.MedicationIngredients)
                                      .FirstOrDefaultAsync(m => m.MedicationId == Id);

            if (medication == null)
            {
                return false;
            }

            // Remove related active ingredients first
            if (medication.MedicationIngredients != null && medication.MedicationIngredients.Any())
            {
                _db.MedicationIngredients.RemoveRange(medication.MedicationIngredients);
            }

            _db.Medications.Remove(medication);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Medication>> GetAllMedications()
        {
            return await _db.Medications
                            .Include(a => a.MedicationIngredients)
                            .ThenInclude(a => a.ActiveIngredient)
                            .Include(d => d.DosageForm)
                            .Include(s => s.Supplier)
                            .OrderBy(m => m.MedicationName)
                            .AsNoTracking()
                            .ToListAsync();
        }


        public async Task<Medication?> GetMedicationByIdAsync(int id)
        {
            return await _db.Medications
                            .Include(a => a.MedicationIngredients)
                            .ThenInclude(a => a.ActiveIngredient)
                            .Include(d => d.DosageForm)
                            .Include(s => s.Supplier)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.MedicationId == id);
        }

        public async Task<bool> UpdateAsync(Medication model)
        {
            var medication = await _db.Medications
                                      .Include(a => a.MedicationIngredients)
                                      .FirstOrDefaultAsync(x => x.MedicationId == model.MedicationId);
            if (medication == null)
            {
                return false;
            }

            // Update basic properties
            medication.MedicationName = model.MedicationName ?? string.Empty;
            medication.DosageFormId = model.DosageFormId;
            medication.SupplierId = model.SupplierId;
            medication.Schedule = model.Schedule;
            medication.ReOrderLevel = model.ReOrderLevel;
            medication.Price = model.Price;
            medication.Quantity = model.Quantity;

            // Update active ingredients
            if (model.MedicationIngredients != null)
            {
                // Remove existing ingredients
                _db.MedicationIngredients.RemoveRange(medication.MedicationIngredients);

                // Add new ingredients
                foreach (var ingredient in model.MedicationIngredients)
                {
                    medication.MedicationIngredients.Add(new MedicationActiveIngredient
                    {
                        ActiveIngredientId = ingredient.ActiveIngredientId,
                        Strength = ingredient.Strength ?? string.Empty,
                        MedicationId = medication.MedicationId
                    });
                }
            }

            await _db.SaveChangesAsync();
            return true;
        }
        public async Task UpdateMedicationAsync(Medication medication)
        {
            // For simple updates like stock quantity
            _db.Medications.Update(medication);
            await _db.SaveChangesAsync();
        }
    }
}