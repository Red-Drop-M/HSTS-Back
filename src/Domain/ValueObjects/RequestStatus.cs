namespace Domain.ValueObjects
{
    public class RequestStatus
    {
        public string Value { get; }

        private RequestStatus(string value)
        {
            Value = value;
        }

        public static RequestStatus Pending() => new RequestStatus("pending");
        public static RequestStatus Resolved() => new RequestStatus("resolved");
        public static RequestStatus Partial() => new RequestStatus("partial");
        public static RequestStatus Cancled() => new RequestStatus("cancled");

        public static RequestStatus Convert(string value) => value.ToLowerInvariant() switch
        {
            "pending" => Pending(),
            "resolved" => Resolved(),
            "partial" => Partial(),
            "cancled" => Cancled(),
            _ => throw new ArgumentException("Invalid RequestStatus", nameof(value))
        };

        public override bool Equals(object? obj)
        {
            if (obj is RequestStatus other)
            {
                return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();
        public override string ToString() => Value;
    }
}
