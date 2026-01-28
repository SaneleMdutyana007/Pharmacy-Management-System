using Microsoft.EntityFrameworkCore;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Data;
using PharmacyManager.Utilities;
using PharmacyManager.Models;

namespace PharmacyManager.Repository
{
    public interface IPharmacyRepository
    {
        Task<Pharmacy> GetPharmacyInfo();
        Task<bool> Add(PharmacyVM model);
        Task<bool> Update(Pharmacy model);
        Task<bool> Delete(Guid Id);
        Task<bool> CheckIfDetailsExist();
    }

    public class PharmacyRepository : IPharmacyRepository
    {
        private readonly IMapper _mapper;
        private readonly PharmacyDbContext _db;

        public PharmacyRepository(PharmacyDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Pharmacy> GetPharmacyInfo()
        {
            return await _db.PharmacyDetails.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<bool> Add(PharmacyVM  model)
        {
           
                var pharmacy = new Pharmacy
                {
                    PharmacyName = model.PharmacyName,
                    HealthCouncilRegNum = model.HealthCouncilRegNum,
                    ResponsiblePharma = model.ResponsiblePharma,
                    PhysicalAddress = model.PhysicalAddress,
                    Contact = model.Contact,
                    Email = model.Email,
                    PharmacyURL = model.PharmacyURL,
                    VAT = model.VAT
                };
                _db.PharmacyDetails.Add(pharmacy);
                await _db.SaveChangesAsync();
                return true;
           
        }

        public async Task<bool> Update(Pharmacy model)
        {
            var pharmacy = await _db.PharmacyDetails.FindAsync(model.Id);

            if (pharmacy == null || model == null){ return false; };
                     
            _db.PharmacyDetails.Update(pharmacy);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(Guid Id)
        {
            var pharmacy = await _db.PharmacyDetails.FindAsync(Id);
            if(pharmacy != null)
            {
                _db.PharmacyDetails.Remove(pharmacy);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> CheckIfDetailsExist()
        {
            return _db.PharmacyDetails.Any();
        }

    }

}
