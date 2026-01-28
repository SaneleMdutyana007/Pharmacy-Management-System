using Microsoft.EntityFrameworkCore;
using PharmacyManager.Data;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Utilities;

namespace PharmacyManager.Repository
{
    public interface IDosageFormRepository
    {
        Task<bool>Create(DosageFormVM model);
        Task<bool> Delete(int Id);
        Task<bool> Update(DosageForm model);
        Task<DosageForm> GetDosageById(int Id);
        Task<IEnumerable<DosageForm>> GetAllDosageForms(); 
    }

    public class DosageFormRepository: IDosageFormRepository
    {
        private readonly IMapper _mapper;
        private readonly PharmacyDbContext _db;

        public DosageFormRepository(IMapper mapper, PharmacyDbContext db)
        {
            _mapper = mapper;
            _db = db;
        }

        public async Task<bool> Create(DosageFormVM model)
        {
            if(model == null)
            {
                return false;
            }
            var dosageForm = _mapper.MapToDosageFormModel(model);
            if(dosageForm == null) {return false; }
            await _db.DosageForms.AddAsync(dosageForm);
            _db.SaveChanges();
            
            return true;
        }

        public async Task<bool> Delete(int Id)
        {
            var dosageForm = await _db.DosageForms.FindAsync(Id);
            if (dosageForm == null) { return false; }
            _db.Remove(dosageForm);
            return true;
        }

        public async Task<IEnumerable<DosageForm>> GetAllDosageForms()
        {
            return await _db.DosageForms
                           .OrderBy(d => d.DosageFormName)
                           .AsNoTracking()
                           .ToListAsync();
        }

        public async Task<DosageForm> GetDosageById(int Id)
        {
            var dosageForm = await _db.DosageForms.AsNoTracking().FirstOrDefaultAsync(x => x.DosageFormId == Id);
            return dosageForm;
        }

        public async Task<bool> Update(DosageForm model)
        {
            var dosageForm = await _db.DosageForms.FindAsync(model.DosageFormId);
            if (dosageForm == null) { return false; }
            dosageForm.DosageFormName = model.DosageFormName;
            _db.Update(dosageForm);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
