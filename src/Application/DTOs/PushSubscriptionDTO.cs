using System;

namespace Application.DTOs
{
    public class PushSubscriptionDTO
    {
        public string Endpoint { get; set; } = string.Empty;
        public KeysInfo Keys { get; set; } = new KeysInfo();
        
        public class KeysInfo
        {
            public string P256dh { get; set; } = string.Empty;
            public string Auth { get; set; } = string.Empty;
        }
    }
}