namespace Domain.ValueObjects
{
    public class BloodBagStatus
    {
        public string Value { get; }

        private BloodBagStatus(string value)
        {
            Value = value;
        }

        public static BloodBagStatus Ready() => new BloodBagStatus("ready");
        public static BloodBagStatus Expired() => new BloodBagStatus("expired");
        public static BloodBagStatus Used() => new BloodBagStatus("used");
        public static BloodBagStatus OutForExpired() => new BloodBagStatus("outforexpired");
        public static BloodBagStatus OutForOther() => new BloodBagStatus("out for other");

        public static BloodBagStatus Convert(string value) => value.ToLowerInvariant() switch
        {
            "ready" => Ready(),
            "expired" => Expired(),
            "used" => Used(),
            "outforexpired" => OutForExpired(),
            "out for other" => OutForOther(),
            _ => throw new ArgumentException("Invalid BloodBagStatus", nameof(value))
        };

        public override bool Equals(object? obj)
        {
            if (obj is BloodBagStatus other)
            {
                return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();
        public override string ToString() => Value;
    }
}
