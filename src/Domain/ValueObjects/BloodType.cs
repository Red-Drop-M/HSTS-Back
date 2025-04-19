using Shared.Exceptions;

namespace Domain.ValueObjects
{
    public class BloodType
    {
        public string Value { get; }

        private BloodType(string value)
        {
            Value = value;
        }

        public static BloodType ANegative() => new BloodType("A-");
        public static BloodType APositive() => new BloodType("A+");
        public static BloodType BNegative() => new BloodType("B-");
        public static BloodType BPositive() => new BloodType("B+");
        public static BloodType ABNegative() => new BloodType("AB-");
        public static BloodType ABPositive() => new BloodType("AB+");
        public static BloodType ONegative() => new BloodType("O-");
        public static BloodType OPositive() => new BloodType("O+");

        public static BloodType FromString(string value) => value.ToUpperInvariant() switch
        {
            "A-" => ANegative(),
            "A+" => APositive(),
            "B-" => BNegative(),
            "B+" => BPositive(),
            "AB-" => ABNegative(),
            "AB+" => ABPositive(),
            "O-" => ONegative(),
            "O+" => OPositive(),
            _ => throw new ValidationException("Invalid BloodType", "BloodType")
        };

        public override bool Equals(object? obj)
        {
            if (obj is BloodType other)
            {
                return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();

        public override string ToString() => Value;
    }
}
