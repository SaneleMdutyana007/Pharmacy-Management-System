using Microsoft.EntityFrameworkCore;
using PharmacyManager.Data;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;

namespace PharmacyManager.Repository
{
    public interface IPrescriptionRepository
    {
        Task<Prescription> AddScript(PrescriptionVM model);
        Task<IEnumerable<Prescription>> GetAllScripts();
        Task<Prescription> GetScriptById(int Id);
        Task<bool> UpdateScript(Prescription model);
        Task<bool> DeleteScript(int Id);
        Task<IEnumerable<Doctor>> GetAllDoctors();
        Task<IEnumerable<Customer>> GetAllPatients();
    }

    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly PharmacyDbContext _db;
        public PrescriptionRepository(PharmacyDbContext db) { _db = db; }

        public async Task<IEnumerable<Prescription>> GetAllScripts()
        {
            return await _db.Prescriptions.Include(m=> m.Medications)
                                          .ThenInclude(m=> m.Medication)
                                          .ThenInclude(a => a.MedicationIngredients)
                                          .AsNoTracking()
                                          .ToListAsync();
        }

        public async Task<Prescription> GetScriptById(int Id)
        {
            return await _db.Prescriptions.Include(m => m.Medications)
                                          .ThenInclude(m => m.Medication)
                                          .ThenInclude(a => a.MedicationIngredients)
                                          .ThenInclude(a => a.ActiveIngredient)
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(x => x.Id == Id);
        }

        public async Task<Prescription> AddScript(PrescriptionVM model)
        {
            if (model == null || model.Medications == null || !model.Medications.Any())
            {
                return null;
            }
            var script = new Prescription 
            { 
                PatientId = model.PatientId,
                DoctorId = model.DoctorId,
                ScriptDate = model.ScriptDate,
                Status = model.Status,
                Medications = model.Medications.Select(m => new PrescriptionMedication
                {
                    MedicationId = m.MedicationId,
                    Quantity = m.Quantity,
                    Instruction = m.Instruction
                }).ToList()
            };            
            
            await _db.Prescriptions.AddAsync(script);
            await _db.SaveChangesAsync();
            return script;
        }

        public async Task<bool> UpdateScript(Prescription model)
        {
            var script = await _db.Prescriptions
                                  .Include(m => m.Medications)
                                  .FirstOrDefaultAsync(script => script.Id == model.Id);
            if (script == null || script.Medications.Count == 0)
            {
                return false;
            }
            script.ScriptDate = model.ScriptDate;
            script.Status = model.Status;           
            script.DoctorId = model.DoctorId;
            script.PatientId = model.PatientId;

            var medsToRemove = script.Medications
        .Where(existingMed => !model.Medications.Any(m => m.Id == existingMed.Id))
        .ToList();

            _db.PrescriptionMedications.RemoveRange(medsToRemove);

            //Update existing meds and add new medication
            foreach (var medication in model.Medications)
            {
                var existingMed = script.Medications.FirstOrDefault(m => m.Id == medication.Id);
                if (existingMed != null)
                {
                    // Update existing medication
                    existingMed.MedicationId = medication.MedicationId;
                    existingMed.Instruction = medication.Instruction?.Trim();
                    existingMed.Quantity = medication.Quantity;
                }
                else
                {
                    // Add new medication
                    script.Medications.Add(new PrescriptionMedication
                    {
                        MedicationId = medication.MedicationId,
                        Instruction = medication.Instruction?.Trim(),
                        Quantity = medication.Quantity
                    });
                }
            }

            _db.Prescriptions.Update(script);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteScript(int Id)
        {
            if(Id <= 0 ) return false;
            var script = await _db.Prescriptions.FindAsync(Id);
            if (script == null) { return false; }
            _db.Remove(script);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Doctor>> GetAllDoctors()
        {
            return await _db.Doctors.ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetAllPatients()
        {
            return await _db.Users.OfType<Customer>().ToListAsync();
        }
    }
}
