using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using PharmacyManager.Areas.Identity.Data;
using PharmacyManager.Data;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Utilities;
using Prototype.Server.Models.Users;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PharmacyManager.Repository
{
    public interface IAccountRepository
    {
        Task<bool> UploadImage(IFormFile image, UserModel user);
        Task<bool> Login(LoginVM model);
        Task<bool> Register(CustomerVM model);
        Task<bool> AddUser(CreateUserVM model);
        Task LogOut(UserModel user);
        Task<Customer> GetCustomer(string customerId);
        Task<IEnumerable<Pharmacist>> GetAllPharmacists();

    }

    public class AccountRepository: IAccountRepository
    {
        private readonly IWebHostEnvironment _web;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountRepository> _logger;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly UserManager<UserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PharmacyDbContext _db;

        public AccountRepository(IWebHostEnvironment web,IMapper mapper, ILogger<AccountRepository> logger, SignInManager<UserModel> signInManager, UserManager<UserModel> userManager, RoleManager<IdentityRole> roleManager, PharmacyDbContext db) 
        {
            _web = web;
            _mapper = mapper;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public async Task<bool> AddUser(CreateUserVM model)
        {
            UserModel user;
            string password = PasswordGenerator.GeneratePassword();

            if (model.Role == nameof(Pharmacist))
            {
                user = new Pharmacist
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    UserName = model.Email,
                    HealthCouncilNumber = model.HealthCouncilNumber,
                    ContactNumber = model.ContactNumber,
                    FirstLogin = true
                };
            }
            else // Manager
            {
                user = new Manager
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    UserName = model.Email,
                    ContactNumber = model.ContactNumber
                };
            }
            await EnsureRoleExistsAsync(model.Role);

            var result = await _userManager.CreateAsync(user, $"{model.Surname}123!");
            if (!result.Succeeded)
                return false;

            await _userManager.AddToRoleAsync(user, model.Role);
            return true;
        }

        public async Task<IEnumerable<Pharmacist>> GetAllPharmacists()
        {
            return await _db.Users.OfType<Pharmacist>().ToListAsync();
        }

        public async Task<Customer> GetCustomer(string customerId)
        {
            return await _db.Users.OfType<Customer>()
                                   .Include(a => a.Allergies)
                                   .ThenInclude(a => a.ActiveIngredient)
                                   .FirstOrDefaultAsync(x => x.Id == customerId);
        }

        public async Task<bool> Login(LoginVM model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded) 
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user?.ImageData != null && user.ImageData.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_web.WebRootPath, "assets", "img", "profile");
                    Directory.CreateDirectory(uploadsFolder);

                    var extension = ".jpg";
                    if (!string.IsNullOrEmpty(user.ImagePath))
                    {
                        var ext = Path.GetExtension(user.ImagePath);
                        if (!string.IsNullOrEmpty(ext))
                            extension = ext;
                    }
                    // Optional: sanitize user name
                    var safeName = Regex.Replace(user.Name, "[^a-zA-Z0-9]", "");
                    var fileName =  $"{safeName}_{user.Id}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    await File.WriteAllBytesAsync(filePath, user.ImageData);

                    // Update path if changed
                    var newImagePath = $"assets/img/profile/{fileName}";
                    if (user.ImagePath != newImagePath)
                    {
                        user.ImagePath = newImagePath;
                        await _userManager.UpdateAsync(user);
                    }
                }
                return true; 
            }
            return false;
        }

        public async Task LogOut(UserModel user)
        {
            if (!string.IsNullOrEmpty(user.ImagePath))
            {       
                var normalizedPath = user.ImagePath.Replace('/', Path.DirectorySeparatorChar);

                var filePath = Path.Combine(_web.WebRootPath, normalizedPath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            await _signInManager.SignOutAsync();
        }

        public async Task<bool> Register(CustomerVM model)
        {
            if (model != null)
            {
                var user = _mapper.MapToCustomerModel(model); //creates a new User instance and maps Customer View model => User (Customer) model             

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var roleExists = await _roleManager.RoleExistsAsync(nameof(Customer));

                    if (!roleExists)
                    {
                        await _roleManager.CreateAsync(new IdentityRole(nameof(Customer)));
                    }

                    await _userManager.AddToRoleAsync(user, nameof(Customer));

                    if (model.allergyIds != null && model.allergyIds.Any())
                    {
                        var validAllergyIds = _db.ActiveIngredients
                                                 .Where(a => model.allergyIds.Contains(a.ActiveIngredientId))
                                                 .Select(a => a.ActiveIngredientId)
                                                 .ToList();

                        var customerAllergies = validAllergyIds.Select(id => new CustomerAllergy
                        {
                            CustomerId = user.Id,
                            ActiveIngredientId = id
                        }).ToList();

                        _db.CustomerAllergies.AddRange(customerAllergies);
                        await _db.SaveChangesAsync();
                    }
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> UploadImage(IFormFile image, UserModel user)
        {
            if(image == null || user == null) return false;

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            var imageData = ms.ToArray();

            user.ImageData = imageData;

            var uploadsFolder = Path.Combine(_web.WebRootPath, "assets", "img", "profile");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(image.FileName);
            var fileName = $"{user.Name}_{user.Id}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            await File.WriteAllBytesAsync(filePath, imageData);

            user.ImagePath = $"assets/img/profile/{fileName}";

            await _userManager.UpdateAsync(user);

            return true;
        }

        private async Task EnsureRoleExistsAsync(string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }           
        }
    }
}
