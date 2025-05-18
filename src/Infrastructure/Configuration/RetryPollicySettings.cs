// Application/Configuration/RetryPolicySettings.cs
namespace Infrastructure.Configuration
{
    public class RetryPolicySettings
    {
        public const string SectionName = "RetryPolicy";
        
        public int MaxRetryCount { get; set; } = 3;
        public int InitialDelayMs { get; set; } = 1000;
        public double BackoffExponent { get; set; } = 2;
    }
}