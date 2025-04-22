using Domain.ValueObjects;

namespace Application.DTOs
{
    public class BloodBagDTO
    {
        public Guid Id { get; set; }
        public BloodType BloodType { get; set; } = BloodType.APositive();
        public BloodBagType BloodBagType { get; set; } = BloodBagType.Blood();
        public DateOnly? ExpirationDate { get; set; }
        public DateOnly? AcquiredDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public Guid DonorId { get; set; }
        public Guid? RequestId { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}