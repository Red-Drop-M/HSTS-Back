namespace Infrastructure.ExternalServices.Kafka
{
    public class KafkaProducer : IDisposable
    {
        private readonly IProducer<Null, string> _producer;
    
        public KafkaProducer(IConfiguration config)
        {
            var producerConfig = new ProducerConfig {
                BootstrapServers = config["Kafka:BootstrapServers"]
            };
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }   

        public async Task PublishBloodRequestAsync(BloodRequestCreatedEvent @event)
        {
            var message = JsonSerializer.Serialize(@event);
            await _producer.ProduceAsync("blood-requests", new Message<Null, string> { Value = message });
        }
        public void Dispose() => _producer?.Dispose();
    }
}