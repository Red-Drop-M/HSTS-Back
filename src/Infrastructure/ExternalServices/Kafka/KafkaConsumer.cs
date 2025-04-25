using Confluent.Kafka;
using Domain.Events;
using Domain.Repositories;
using System.Text.Json;
using Microsoft.Extensions.Logging;
namespace Infrastructure.ExternalServices.Kafka
{
    public class KafkaConsumerService : BackgroundService
    {   
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IServiceProvider _services;

        public KafkaConsumerService(IConfiguration config, IServiceProvider services, ILogger<KafkaConsumerService> logger)
        {
            var consumerConfig = new ConsumerConfig {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = "bloodbank-service",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _logger = logger;
            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            _consumer.Subscribe("donor-pledges");
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try 
                {
                    var result = _consumer.Consume(ct);
                    var pledge = JsonSerializer.Deserialize<DonorPledgeEvent>(result.Message.Value);
                
                    using var scope = _services.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IPledgeRepository>();
                    
                }
                catch (Exception ex)
                {
                // Handle errors
                _logger.LogError(ex, "Error consuming message from Kafka");
                
                }
                finally
                {
                    _consumer.Close();
                    _consumer.Dispose();
                    _logger.LogInformation("Kafka consumer closed");
                    _logger.LogInformation("Kafka consumer disposed");
                    _logger.LogInformation("Kafka consumer stopped");
                }
            }
        }
    }
}