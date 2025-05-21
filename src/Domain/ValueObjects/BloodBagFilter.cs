namespace Domain.ValueObjects
{
    public class BloodBagFilter
    {
        public BloodType? BloodType { get; set; }
        // Add this new property for multiple blood types
        public List<string>? BloodTypes { get; set; }
        public BloodBagType? BloodBagType { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public DateOnly? AcquiredDate { get; set; }
        public BloodBagStatus? Status { get; set; }
        public Guid? DonorId { get; set; }
        public Guid? RequestId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}