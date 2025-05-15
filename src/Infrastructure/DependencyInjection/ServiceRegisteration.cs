using Application.interfaces;
using Domain.Repositories;
using Infrastructure.ExternalServices.Kafka;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.DependencyInjection
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Register Repositories
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBloodBagRepository, BloodBagRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IDonorRepository, DonorRepository>();
            services.AddScoped<IGlobalStockRepository, GlobalStockRepository>();
            services.AddScoped<IPledgeRepository, PledgeRepository>();

            // Register Kafka Services
            services.AddKafkaServices(configuration);

            return services;
        }

        private static IServiceCollection AddKafkaServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind Kafka configuration from appsettings.json
            services.Configure<KafkaSettings>(
                configuration.GetSection(KafkaSettings.SectionName));

            // Register Kafka components
            services.AddSingleton<KafkaTopicInitializer>();
            services.AddScoped<IEventProducer, KafkaEventPublisher>();
            services.AddHostedService<KafkaConsumerService>();

            return services;
        }
    }
}