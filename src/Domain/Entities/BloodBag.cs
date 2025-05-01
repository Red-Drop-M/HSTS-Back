using Domain.ValueObjects;

namespace Domain.Entities
{
    public class BloodBag
    {
        public Guid Id { get; private set; }
        public BloodBagType BloodBagType { get; private set; } = BloodBagType.Blood();
        public BloodType BloodType { get; private set; } = BloodType.APositive();
        public BloodBagStatus Status { get; private set; } = BloodBagStatus.Ready();
        public DateOnly? ExpirationDate { get; private set; }
        public DateOnly? AcquiredDate { get; private set; } = DateOnly.FromDateTime(DateTime.Now);

        // Foreign keys
        public Guid? DonorId { get; private set; }
        public Guid? RequestId { get; private set; }

        // Navigation properties
        public Donor? Donor { get; private set; }
        public Request? Request { get; private set; }

        private BloodBag() { }

        public BloodBag(
            BloodBagType bloodBagType,
            BloodType bloodType,
            BloodBagStatus status,
            DateOnly? expirationDonorDate,
            DateOnly? acquiredDate,
            Guid donorId,
            Guid? requestId = null)
        {
            BloodBagType = bloodBagType;
            BloodType = bloodType;
            Status = status;
            ExpirationDate = expirationDonorDate;
            AcquiredDate = acquiredDate ?? DateOnly.FromDateTime(DateTime.Now);
            DonorId = donorId;
            RequestId = requestId;
        }

        public void UpdateDetails(
            BloodBagType? bloodBagType = null,
            BloodType? bloodType = null,
            DateOnly? expirationDate = null,
            DateOnly? acquiredDate = null,
            Guid? donorId = null,
            Guid? requestId = null)
        {
            if (bloodBagType is not null) BloodBagType = bloodBagType;
            if (bloodType is not null) BloodType = bloodType;
            if (expirationDate is not null) ExpirationDate = expirationDate.Value;
            if (acquiredDate is not null) AcquiredDate = acquiredDate.Value;
            if (donorId is not null) DonorId = donorId;
            if (requestId is not null) RequestId = requestId;
        }

        public void UpdateStatus(BloodBagStatus status)
        {
            Status = status;
        }
        public void UpdateStatus(BloodBagStatus status, DateOnly expirationDate)
        {
            Status = status;
            ExpirationDate = expirationDate;
        }
    }
}
