using Microsoft.EntityFrameworkCore;
using PharmacyManager.Data;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Utilities;

namespace PharmacyManager.Repositories
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllSuppliers();
        Task<Supplier?> GetSupplierByIdAsync(int id);
        Task<Supplier> CreateAsync(SupplierVM model);
        Task<bool> UpdateAsync(Supplier model);
        Task<bool> DeleteAsync(int Id);
        public class SupplierRepository : ISupplierRepository
        {
            private readonly PharmacyDbContext _db;
            private readonly IMapper _mapper;

            public SupplierRepository(PharmacyDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }
            public async Task<IEnumerable<Supplier>> GetAllSuppliers()
            {
                return await _db.Suppliers
                                .OrderBy(s => s.SupplierName)
                                .AsNoTracking()
                                .ToListAsync();
            }
            public async Task<Supplier?> GetSupplierByIdAsync(int id)
            {
                return await _db.Suppliers
                                .AsNoTracking()
                                .FirstOrDefaultAsync(s => s.SupplierId == id);
            }

            public async Task<Supplier> CreateAsync(SupplierVM model)
            {
                var supplier = new Supplier
                {
                    SupplierName = model.SupplierName ?? string.Empty,
                    ContactPerson = model.ContactPerson ?? string.Empty,
                    Email = model.Email ?? string.Empty,
                };

                _db.Suppliers.Add(supplier);
                await _db.SaveChangesAsync();
                return supplier;
            }

            public async Task<bool> UpdateAsync(Supplier model)
            {
                var supplier = await _db.Suppliers
                                        .FirstOrDefaultAsync(s => s.SupplierId == model.SupplierId);
                if (supplier == null)
                {
                    return false;
                }

                supplier.SupplierName = model.SupplierName;
                supplier.ContactPerson = model.ContactPerson;
                supplier.Email = model.Email;

                await _db.SaveChangesAsync();
                return true;
            }
            public async Task<bool> DeleteAsync(int Id)
            {
                var supplier = await _db.Suppliers.FindAsync(Id);
                if (supplier == null)
                {
                    return false;
                }

                _db.Suppliers.Remove(supplier);
                await _db.SaveChangesAsync();
                return true;
            }
        }
    }
}
