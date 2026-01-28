using Microsoft.EntityFrameworkCore;
using PharmacyManager.Data;

namespace PharmacyManager.Services
{
    public static class RegisterDatabaseServiceExtension
    {
        public static IServiceCollection RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Get your connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Register your DbContext with SQL Server provider
            services.AddDbContext<PharmacyDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add EF Core developer exception filter (optional)
            services.AddDatabaseDeveloperPageExceptionFilter();
            return services;
        }
    }
}
