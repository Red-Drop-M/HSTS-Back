using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices.Kafka
{
    public class KafkaTopicInitializer
    {
        private readonly KafkaSettings _settings;

        public KafkaTopicInitializer(IOptions<KafkaSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task InitializeAsync()
        {
            using var adminClient = new AdminClientBuilder(
                new AdminClientConfig { BootstrapServers = _settings.BootstrapServers }
            ).Build();

            foreach (var topicEntry in _settings.Topics)
            {
                var topicSpec = new TopicSpecification
                {
                    Name = topicEntry.Value,
                    NumPartitions = 3,
                    ReplicationFactor = 1
                };

                try
                {
                    await adminClient.CreateTopicsAsync(new[] { topicSpec });
                    await Task.Delay(1000); // Allow time for topic creation
                }
                catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
                {
                    // Topic exists - safe to ignore
                }
            }
        }
    }
}