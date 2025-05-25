namespace Infrastructure.Configuration
{
    public class WebPushOptions
    {
        public string PublicKey { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
    }
}