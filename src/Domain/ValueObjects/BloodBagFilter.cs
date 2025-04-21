namespace Domain.ValueObjects
{
    public class BloodBagFilter
    {
        public BloodType? BloodType { get; set; }
        public BloodBagType? BloodBagType { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateOnly? AcquiredDate { get; set; }
        public BloodBagStatus? Status { get; set; }
        public Guid? DonorId { get; set; }
        public Guid? RequestId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}