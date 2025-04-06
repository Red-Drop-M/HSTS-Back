namespace Domain.ValueObjects
{
    public class Priority
    {
        public string Value { get; }

        private Priority(string value)
        {
            Value = value;
        }

        public static Priority Critical() => new Priority("critical");
        public static Priority Low() => new Priority("low");
        public static Priority Standard() => new Priority("standard");

        public static Priority Convert(string value) => value.ToLowerInvariant() switch
        {
            "critical" => Critical(),
            "low" => Low(),
            "standard" => Standard(),
            _ => throw new ArgumentException("Invalid Priority", nameof(value))
        };

        public override bool Equals(object? obj)
        {
            if (obj is Priority other)
            {
                return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();
        public override string ToString() => Value;
    }
}
