using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalServices.Kafka
{
    public class KafkaTopicInitializer
    {
        private readonly KafkaSettings _settings;
        private readonly ILogger<KafkaTopicInitializer> _logger;

        public KafkaTopicInitializer(
            IOptions<KafkaSettings> settings, 
            ILogger<KafkaTopicInitializer> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Starting Kafka topic initialization with broker: {Broker}", _settings.BootstrapServers);
            
            using var adminClient = new AdminClientBuilder(
                new AdminClientConfig { BootstrapServers = _settings.BootstrapServers }
            ).Build();

            foreach (var topicEntry in _settings.Topics)
            {
                _logger.LogInformation("Creating topic: {TopicName} -> {ActualTopic}", topicEntry.Key, topicEntry.Value);
                
                var topicSpec = new TopicSpecification
                {
                    Name = topicEntry.Value,
                    NumPartitions = 3,
                    ReplicationFactor = 1
                };

                try
                {
                    await adminClient.CreateTopicsAsync(new[] { topicSpec });
                    _logger.LogInformation("Successfully created topic: {Topic}", topicEntry.Value);
                    await Task.Delay(1000); // Allow time for topic creation
                }
                catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
                {
                    _logger.LogInformation("Topic already exists: {Topic}", topicEntry.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create topic: {Topic}", topicEntry.Value);
                    // Continue trying other topics
                }
            }
        }
    }
}