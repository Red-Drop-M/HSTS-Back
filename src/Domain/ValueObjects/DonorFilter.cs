namespace Domain.ValueObjects
{
    public class DonorFilter
    {
        public string? Name { get; set; }
        public BloodType? BloodType { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? NIN { get; set; } 
        public DateOnly? LastDonationDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}