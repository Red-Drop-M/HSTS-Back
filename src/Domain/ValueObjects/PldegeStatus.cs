namespace Domain.ValueObjects
{
    public sealed class PledgeStatus
    {
        public static PledgeStatus Pledged = new("Pledged");
        public static PledgeStatus Fulfilled = new("Fulfilled");
        public static PledgeStatus Canceled = new("Canceled");

        public string Value { get; }

        private PledgeStatus(string value)
        {
            Value = value;
        }

        public static PledgeStatus FromString(string status)
        {
            return status switch
            {
                "Pledged" => Pledged,
                "Fulfilled" => Fulfilled,
                "Canceled" => Canceled,
                _ => throw new ArgumentException($"Invalid pledge status: {status}")
            };
        }

        public IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}