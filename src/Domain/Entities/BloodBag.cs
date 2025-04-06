using Domain.ValueObjects;

namespace Domain.Entities
{
    public class BloodBag
    {
        // Primary Key
        public Guid Id { get; private set; }

        // Value Objects
        public BloodType BloodType { get; private set; } = BloodType.APositive();
        public DateOnly AcquiredDate { get; private set; } = DateOnly.FromDateTime(DateTime.Now);
        public BloodBagType BloodBagType { get; private set; } = BloodBagType.Blood();
        public DateTime ExpirationDonorDate { get; private set; }
        public BloodBagStatus Status { get; private set; } = BloodBagStatus.Ready();

        // Foreign Keys
        public Guid DonorId { get; private set; }
        public Guid? RequestId { get; private set; }

        // Navigation Properties
        public Donor Donor { get; private set; } = null!;
        public Request? Request { get; private set; }

        // Required by EF Core
        private BloodBag() { }

        // Optional constructor for manual creation
        public BloodBag(
            BloodType bloodType,
            BloodBagType bloodBagType,
            DateTime expirationDonorDate,
            Guid donorId,
            Guid? requestId = null)
        {
            BloodType = bloodType;
            BloodBagType = bloodBagType;
            ExpirationDonorDate = expirationDonorDate;
            DonorId = donorId;
            RequestId = requestId;
            AcquiredDate = DateOnly.FromDateTime(DateTime.Now);
            Status = BloodBagStatus.Ready();
        }

        // You can add methods like SetStatus(), AssignRequest(), etc. later.
    }
}
