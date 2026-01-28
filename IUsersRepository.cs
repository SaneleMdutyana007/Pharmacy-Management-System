using PharmacyManager.Areas.Identity.Data;
using PharmacyManager.Data;
using PharmacyManager.Models;
using Microsoft.EntityFrameworkCore;
public interface IUserRepository
{
    Task<IEnumerable<UserModel>> GetAllUsers();
    Task<IEnumerable<Pharmacist>> GetAllPharmacist();
    Task<IEnumerable<Customer>> GetAllCustomers();
    Task<IEnumerable<Manager>> GetAllManagers();

    Task<Pharmacist> GetPharmacistById(string Id);
    Task<Customer> GetCustomerById(string Id);
    Task<Manager> GetManagerById(string Id);
    Task<UserModel> GetUserById(string Id);
    Task<bool> UpdateUser(UserModel user);
    Task<bool> DeleteUser(string Id);
}

public class UserRepository : IUserRepository
{
    private readonly PharmacyDbContext _db;
    public UserRepository(PharmacyDbContext db) { _db = db; }

    public async Task<IEnumerable<UserModel>> GetAllUsers()
    {
        var users = await _db.Users
            .Select(u => new UserModel
            {
                Id = u.Id,
                Name = u.Name,
                Surname = u.Surname,
                Email = u.Email,
                ContactNumber = u.ContactNumber,
                UserType = u is Pharmacist ? "Pharmacist" :
                          u is Customer ? "Customer" : "Manager"
            })
            .ToListAsync();
        return users;
    }

    public async Task<IEnumerable<Customer>> GetAllCustomers()
    {
        return await _db.Users.OfType<Customer>().ToListAsync();
    }

    public async Task<IEnumerable<Manager>> GetAllManagers()
    {
        return await _db.Users.OfType<Manager>().ToListAsync();
    }

    public async Task<IEnumerable<Pharmacist>> GetAllPharmacist()
    {
        return await _db.Users.OfType<Pharmacist>().ToListAsync();
    }

    public async Task<Customer> GetCustomerById(string Id)
    {
        return await _db.Users.OfType<Customer>().FirstOrDefaultAsync(x => x.Id == Id);
    }

    public async Task<Manager> GetManagerById(string Id)
    {
        return await _db.Users.OfType<Manager>().FirstOrDefaultAsync(x => x.Id == Id);
    }

    public async Task<Pharmacist> GetPharmacistById(string Id)
    {
        return await _db.Users.OfType<Pharmacist>().FirstOrDefaultAsync(x => x.Id == Id);
    }

    public async Task<UserModel> GetUserById(string Id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == Id);
        if (user == null) return null;

        return new UserModel
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            ContactNumber = user.ContactNumber,
            UserType = user is Pharmacist ? "Pharmacist" :
                      user is Customer ? "Customer" : "Manager"
        };
    }

    public async Task<bool> UpdateUser(UserModel userModel)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userModel.Id);
        if (user == null) return false;

        user.Name = userModel.Name;
        user.Surname = userModel.Surname;
        user.Email = userModel.Email;
        user.ContactNumber = userModel.ContactNumber;

        _db.Users.Update(user);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteUser(string Id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == Id);
        if (user == null) return false;

        _db.Users.Remove(user);
        return await _db.SaveChangesAsync() > 0;
    }
}