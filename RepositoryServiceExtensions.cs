using PharmacyManager.Repositories;
using PharmacyManager.Repository;
using PharmacyManager.Utilities;

namespace PharmacyManager.Services
{
    public static class RepositoryServiceExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IPharmacyRepository, PharmacyRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IActiveIngredientRepository, ActiveIngredientRepository>();
            services.AddScoped<IDosageFormRepository, DosageFormRepository>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IMedicationRepository, MedicationRepository>();
            services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
            services.AddScoped<ISupplierRepository, ISupplierRepository.SupplierRepository>();
            services.AddScoped<IPDFRespository, PDFRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IMapper, Mapper>();
            services.AddScoped<PDFService>();
            services.AddScoped<EmailService>();

            return services;
        }
    }
}
