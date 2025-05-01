// Infrastructure/Configuration/KafkaSettings.cs
namespace Infrastructure.ExternalServices.Kafka;

public class KafkaSettings
{
    public const string SectionName = "Kafka";
    
    public string BootstrapServers { get; set; } = "localhost:9092";
    public Dictionary<string, string> Topics { get; set; } = new();
    
    // Optional: Add producer-specific settings
    public int MessageTimeoutMs { get; set; } = 5000;
    public bool EnableIdempotence { get; set; } = true;
}