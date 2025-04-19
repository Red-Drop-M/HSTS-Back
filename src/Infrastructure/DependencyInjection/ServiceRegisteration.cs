using Microsoft.Extensions.DependencyInjection;
using Domain.Repositories;
using Infrastructure.Repositories;
using Domain.Entities;

namespace Infrastructure.DependencyInjection
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Register Repositories
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBloodBagRepository, BloodBagRepository>();
            services.AddScoped<IServiceRepository,ServiceRepository>();
            services.AddScoped<IDonorRepository, DonorRepository>();
            services.AddScoped<IGlobalStockRepository, GlobalStockRepository>();

            return services;
        }
    }
}
