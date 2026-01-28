using Microsoft.AspNetCore.Identity;
using PharmacyManager.Areas.Identity.Data;
using PharmacyManager.Models;
using PharmacyManager.Utilities;

namespace PharmacyManager.Services
{
    public static class SeedCustomerService
    {
        public static async Task SeedCustomerUser(IServiceProvider service)
        {
            using var scope = service.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
            string firstName = "Tyrel";
            string lastName = "Larnista";
            string email = "customer@gmail.com";
            string password = PasswordGenerator.GeneratePassword();
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new Customer();
                user.Name = firstName;
                user.Surname = lastName;
                user.UserName = email;
                user.Email = email;
                user.EmailConfirmed = true;
                user.SouthAfricanID = "xxxxxxxxxxxxx";

                var result = await userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, nameof(Customer));
                }
            }
        }
    }
}
