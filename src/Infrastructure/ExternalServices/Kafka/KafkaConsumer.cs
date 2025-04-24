namespace Infrastructure.ExternalServices.Kafka
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IServiceProvider _services;

        public KafkaConsumerService(IConfiguration config, IServiceProvider services)
        {
            var consumerConfig = new ConsumerConfig {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = "bloodbank-service",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        
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
                    var pledge = JsonSerializer.Deserialize<DonorPledgedEvent>(result.Message.Value);
                
                    using var scope = _services.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IPledgeRepository>();
                    await repo.AddPledgeAsync(pledge.DonorId, pledge.RequestId);
                }
                catch (Exception ex)
                {
                // Handle errors
                }
            }
        }
    }
}