using Microsoft.AspNetCore.Identity;
using PharmacyManager.Areas.Identity.Data;
using PharmacyManager.Data;

namespace PharmacyManager.Services
{
    public static class RegisterIdentityServiceExtension
    {
        public static IServiceCollection RegisterIdentity(this IServiceCollection services)
        {
            // Configure Identity with your custom user and context
            services.AddDefaultIdentity<UserModel>(options =>
            {
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;            
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<PharmacyDbContext>();

            return services;
        }
    }
}
