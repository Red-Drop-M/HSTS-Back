using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.ValueObjects
{
    public sealed class PledgeStatus
    {
        private static readonly Dictionary<string, PledgeStatus> _statuses = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Pledged"] = new PledgeStatus("Pledged"),
            ["Fulfilled"] = new PledgeStatus("Fulfilled"),
            ["Canceled"] = new PledgeStatus("Canceled"),
            ["expired"] = new PledgeStatus("expired"),
        };

        public static PledgeStatus Pledged => _statuses["Pledged"];
        public static PledgeStatus Fulfilled => _statuses["Fulfilled"];
        public static PledgeStatus Canceled => _statuses["Canceled"];

        public string Value { get; }

        private PledgeStatus(string value)
        {
            Value = value;
        }

        public static PledgeStatus FromString(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ValidationException("Status cannot be empty", nameof(status));

            if (_statuses.TryGetValue(status.Trim(), out var pledgeStatus))
                return pledgeStatus;

            throw new ValidationException(
                $"Invalid status: '{status}'. Valid values are: {string.Join(", ", _statuses.Keys)}",
                nameof(status));
        }

        public override string ToString() => Value;

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override bool Equals(object obj)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            return obj is PledgeStatus other && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
            => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        public static implicit operator string(PledgeStatus status) => status.Value;
        public static explicit operator PledgeStatus(string status) => FromString(status);

        public bool IsPledged => Equals(Pledged);
        public bool IsFulfilled => Equals(Fulfilled);
        public bool IsCanceled => Equals(Canceled);
    }
}
