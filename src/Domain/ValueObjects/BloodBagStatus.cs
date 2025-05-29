namespace Domain.ValueObjects
{
    public class BloodBagStatus
    {
        public string Value { get; private set; }

        private BloodBagStatus(string value)
        {
            Value = value;
        }

        public static BloodBagStatus Acquired() => new BloodBagStatus("acquired");
        public static BloodBagStatus Ready() => new BloodBagStatus("ready");
        public static BloodBagStatus Used() => new BloodBagStatus("used");
        public static BloodBagStatus Expired() => new BloodBagStatus("expired");
        public static BloodBagStatus OutForOther() => new BloodBagStatus("outforother");
        
        // For backward compatibility
        public static BloodBagStatus Aquired() => Acquired();
        
        // Add Convert method for string conversion (used in endpoints)
        public static BloodBagStatus Convert(string value)
        {
            return FromString(value);
        }
        
        // Factory method for creating from string
        public static BloodBagStatus FromString(string value)
        {
            return value?.ToLower() switch
            {
                "acquired" => Acquired(),
                "ready" => Ready(),
                "used" => Used(),
                "expired" => Expired(),
                "outforother" => OutForOther(),
                // Add backward compatibility for the old spelling
                "aquired" => Acquired(),
                _ => throw new ArgumentException($"Invalid blood bag status: {value}")
            };
        }

        // For EF Core
        protected BloodBagStatus() { Value = string.Empty; }
    }
}
