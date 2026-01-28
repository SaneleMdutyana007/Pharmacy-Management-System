using Microsoft.EntityFrameworkCore;
using PharmacyManager.Data;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Utilities;

namespace PharmacyManager.Repository
{
    public interface IDoctorRepository
    {
        Task<bool> CreateAsync(DoctorVM model);
        Task<bool> UpdateAsync(Doctor model);
        Task<bool> DeleteAsync(int id);
        Task<Doctor?> GetDoctorByIdAsync(int id);
        Task<IEnumerable<Doctor>> GetAllDoctors();

        // Keep old methods for backward compatibility
        Task<bool> AddAsync(DoctorVM model);
        Task<Doctor> GetDoctorById(int id);
    }
    public class DoctorRepository : IDoctorRepository
    {
        private readonly IMapper _mapper;
        private readonly PharmacyDbContext _db;
        public DoctorRepository(IMapper mapper, PharmacyDbContext db) { _db = db; _mapper = mapper; }

        public async Task<bool> CreateAsync(DoctorVM model)
        {
            var doctor = new Doctor
            {
                DoctorName = model.DoctorName ?? string.Empty,
                DoctorSurname = model.DoctorSurname ?? string.Empty,
                PracticeNumber = model.PracticeNumber ?? string.Empty,
                PhoneNumber = model.PhoneNumber ?? string.Empty,
                Email = model.Email ?? string.Empty
            };

            await _db.Doctors.AddAsync(doctor);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(Doctor model)
        {
            var doctor = await _db.Doctors.FindAsync(model.DoctorId);
            if (doctor == null)
            {
                return false;
            }

            doctor.DoctorName = model.DoctorName ?? string.Empty;
            doctor.DoctorSurname = model.DoctorSurname ?? string.Empty;
            doctor.PracticeNumber = model.PracticeNumber ?? string.Empty;
            doctor.PhoneNumber = model.PhoneNumber ?? string.Empty;
            doctor.Email = model.Email ?? string.Empty;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var doctor = await _db.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return false;
            }

            _db.Doctors.Remove(doctor);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int id)
        {
            return await _db.Doctors
                           .AsNoTracking()
                           .FirstOrDefaultAsync(x => x.DoctorId == id);
        }

        public async Task<IEnumerable<Doctor>> GetAllDoctors()
        {
            return await _db.Doctors
                           .AsNoTracking()
                           .ToListAsync();
        }

        // Backward compatibility methods
        public async Task<bool> AddAsync(DoctorVM model)
        {
            return await CreateAsync(model);
        }

        public async Task<Doctor> GetDoctorById(int id)
        {
            return await GetDoctorByIdAsync(id) ?? new Doctor();
        }
    }
}
