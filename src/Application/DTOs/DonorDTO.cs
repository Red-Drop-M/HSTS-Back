using Domain.ValueObjects;

namespace Application.DTOs
{
    public class DonorDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public BloodType BloodType { get; set; } = BloodType.APositive();
        public DateOnly LastDonationDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string Address { get; set; } = string.Empty;
        public string NIN { get; set; } = string.Empty; // National ID Number

        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        

    }

} 