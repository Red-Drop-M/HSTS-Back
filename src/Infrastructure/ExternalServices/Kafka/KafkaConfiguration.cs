// Infrastructure/Configuration/KafkaSettings.cs
namespace Infrastructure.ExternalServices.Kafka
{
    public class KafkaSettings
    {
        public const string SectionName = "Kafka";
        
        public required string BootstrapServers { get; set; }
        public required Dictionary<string, string> Topics { get; set; }
        public List<string> ConsumerTopics { get; set; } = new();
        public bool EnableIdempotence { get; set; }
        public required string GroupId { get; set; }
        public required string AutoOffsetReset { get; set; }
    }
}
