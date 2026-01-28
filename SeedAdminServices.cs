using Microsoft.AspNetCore.Identity;
using PharmacyManager.Areas.Identity.Data;
using PharmacyManager.Models;
using PharmacyManager.Utilities;
using SQLitePCL;

namespace PharmacyManager.Services
{
    public static class SeedAdminServices
    {
        public async static Task SeedAdminUser(IServiceProvider service)
        {
            using var scope = service.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
            string firstName = "James";
            string lastName = "Morgan";
            string email = "administration2@gmail.com";
            string password = "Password123!";

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new Admin();
                user.Name = firstName;
                user.Surname = lastName;
                user.UserName = email;
                user.Email = email;
                user.EmailConfirmed = true;
                user.SouthAfricanID = "xxxxxxxxxxxxx";
      
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, nameof(Admin));
                }
            }
        }
    }
}
