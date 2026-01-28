namespace PharmacyManager.Services
{
    public static class RegisterSessionsServiceExtension
    {
        public static IServiceCollection RegisterSessions(this IServiceCollection service)
        {
            service.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            return service;
        }
    }
}
