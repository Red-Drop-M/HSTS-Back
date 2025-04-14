using Domain.ValueObjects;

namespace Domain.Entities
{
    public class BloodBag
    {
        public Guid Id { get; private set; }
        public BloodType BloodType { get; private set; } = BloodType.APositive();
        public BloodBagType BloodBagType { get; private set; } = BloodBagType.Blood();
        public BloodBagStatus Status { get; private set; } = BloodBagStatus.Ready();
        public DateOnly AcquiredDate { get; private set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly ExpirationDate { get; private set; }

        // Foreign keys
        public Guid? DonorId { get; private set; }
        public Guid? RequestId { get; private set; }

        // Navigation properties
        public Donor? Donor { get; private set; }
        public Request? Request { get; private set; }

        private BloodBag() { }

        public BloodBag(
            BloodType bloodType,
            BloodBagType bloodBagType,
            DateOnly expirationDonorDate,
            Guid donorId,
            Guid? requestId = null)
        {
            BloodType = bloodType;
            BloodBagType = bloodBagType;
            ExpirationDate = expirationDonorDate;
            DonorId = donorId;
            RequestId = requestId;
            AcquiredDate = DateOnly.FromDateTime(DateTime.Now);
            Status = BloodBagStatus.Ready();
        }
    }
}
