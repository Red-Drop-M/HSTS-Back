using Shared.Exceptions;

namespace Domain.ValueObjects
{
    public class BloodBagType
    {
        public string Value { get; }

        private BloodBagType(string value)
        {
            Value = value;
        }

        public static BloodBagType Blood() => new BloodBagType("blood");
        public static BloodBagType Plaquette() => new BloodBagType("plaquette");
        public static BloodBagType Plasma() => new BloodBagType("plasma");

        public static BloodBagType Convert(string value) => value.ToLowerInvariant() switch
        {
            "blood" => Blood(),
            "plaquette" => Plaquette(),
            "plasma" => Plasma(),
            _ => throw new ArgumentException("Invalid BloodBagType", nameof(value))
        };

        public override bool Equals(object? obj)
        {
            if (obj is BloodBagType other)
            {
                return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();
        public override string ToString() => Value;
        public static string convert(string value)
        {
            return value.ToLowerInvariant() switch
            {
                "blood" => Blood().ToString(),
                "plaquette" => Plaquette().ToString(),
                "plasma" => Plasma().ToString(),
                _ => throw new InternalServerException("Invalid BloodBagType", "BLood BagType")
            };

        }
    }
}
