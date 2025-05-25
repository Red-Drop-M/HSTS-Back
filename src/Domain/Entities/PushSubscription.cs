using System;

namespace Domain.Entities
{
    public class PushSubscription
    {
        public Guid Id { get; private set; }
        public Guid? UserId { get; private set; }
        public string Endpoint { get; private set; } = string.Empty;
        public string P256dh { get; private set; } = string.Empty;
        public string Auth { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastModifiedAt { get; private set; }
        
        private PushSubscription() { } // For EF Core
        
        public PushSubscription(Guid? userId, string endpoint, string p256dh, string auth)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Endpoint = endpoint;
            P256dh = p256dh;
            Auth = auth;
            CreatedAt = DateTime.UtcNow;
        }
        
        public void Update(string endpoint, string p256dh, string auth)
        {
            Endpoint = endpoint;
            P256dh = p256dh;
            Auth = auth;
            LastModifiedAt = DateTime.UtcNow;
        }
    }
}