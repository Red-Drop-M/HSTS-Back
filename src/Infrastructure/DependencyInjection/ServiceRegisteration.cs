using Application.Interfaces;
using Domain.Repositories;
using Infrastructure.ExternalServices.Kafka;
using Infrastructure.Repositories;
using MediatR;
using Domain.Events;
using Application.Features.EventHandling.Handlers;


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

            // Register Kafka Services with MediatR integration
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

            // Add MediatR (if not already added elsewhere)
            services.AddMediatR(typeof(Application.Features.EventHandling.Handlers.DonorPledgeEventHandler).Assembly);

            // Register Kafka components
            services.AddSingleton<ITopicDispatcher, TopicDispatcher>();
            services.AddSingleton<KafkaTopicInitializer>();
            services.AddScoped<IEventProducer, KafkaEventPublisher>();
            services.AddHostedService<KafkaConsumerService>();

            // Register topic handlers
            services.AddTransient(provider => 
            {
                var dispatcher = provider.GetRequiredService<ITopicDispatcher>();
                
                // Map topics to MediatR commands - use the correct command classes that implement IRequest
                dispatcher.Register<Application.Features.BloodRequests.Commands.DonorPledgeCommand>(
                    "donors-pledges");  // Match the topic name in appsettings.json
          // Match the topic name in appsettings.json
                
                return dispatcher;
            });

            return services;
        }
    }
}