using Application.Interfaces;
using Domain.Repositories;
using Infrastructure.ExternalServices.Kafka;
using Infrastructure.Repositories;
using MediatR;
using Infrastructure.ExternalServices;
using Application.Features.EventHandling.Commands;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure.DependencyInjection
{
        public static class ServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register core repositories
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBloodBagRepository, BloodBagRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IDonorRepository, DonorRepository>();
            services.AddScoped<IGlobalStockRepository, GlobalStockRepository>();
            services.AddScoped<IPledgeRepository, PledgeRepository>();

            // Configure Kafka services
            services.AddKafkaServices(configuration);

            return services;
        }

        private static IServiceCollection AddKafkaServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind Kafka configuration
            services.Configure<KafkaSettings>(
                configuration.GetSection(KafkaSettings.SectionName));

            // Register MediatR with explicit handler assembly
            services.AddMediatR(typeof(DonorPledgeCommand).Assembly);

            // Kafka infrastructure components
            services.AddSingleton<ITopicDispatcher, TopicDispatcher>();
            services.AddSingleton<KafkaTopicInitializer>();
            services.AddScoped<IEventProducer, KafkaEventPublisher>();
            services.AddHostedService<KafkaConsumerService>();

            // Health check registration
            services.AddHealthChecks()
                .AddKafka(new Confluent.Kafka.ProducerConfig 
                { 
                    BootstrapServers = configuration.GetSection("Kafka:BootstrapServers").Value 
                })
                .AddNpgSql(configuration.GetConnectionString("DefaultConnection"));

            return services;
        }
    }
}