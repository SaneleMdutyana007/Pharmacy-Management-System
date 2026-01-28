using Microsoft.AspNetCore.Identity;
using PharmacyManager.Utilities;

namespace PharmacyManager.Services
{
    public static class SeedRoleServices
    {
        public static async Task SeedRoles(IServiceProvider service)
        {
            using var scope = service.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roles = new[]
            {

                Roles.Admin,
                Roles.Customer

            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
