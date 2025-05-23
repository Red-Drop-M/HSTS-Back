using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Donor
    {
        // Primary Key (generated by the database)
        public Guid Id { get; private set; }

        // Donor Info
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public BloodType BloodType { get; private set; } = BloodType.APositive();
        public DateOnly? LastDonationDate { get; private set; }

        public DateOnly? DateOfBirth { get; private set; }
        public string NIN { get; private set; } = string.Empty; // National ID Number
        public string PhoneNumber { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        // Navigation Property (One-to-Many with BloodBags)
        public ICollection<BloodBag> BloodBags { get; private set; } = new List<BloodBag>();
        public ICollection<Request> Requests { get; private set; } = new List<Request>();
        public ICollection<DonorPledge> Pledges { get; private set; } = new List<DonorPledge>();
        private Donor() { }

        // Optional constructor to create a donor (without setting Id)
        public Donor(
            string name,
            string email,
            BloodType bloodType,
            DateOnly? lastDonationDate,
            string address,
            string nin,
            string phoneNumber,
            DateOnly dateOfBirth)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            DateOfBirth = dateOfBirth;
            BloodType = bloodType;
            NIN = nin;
            PhoneNumber = phoneNumber;
            Address = address;
            LastDonationDate = lastDonationDate;
        }

        public void UpdateDetails(
            string? name,
            string? email,
            BloodType? bloodType,
            DateOnly? lastDonationDate,
            string? address,
            string? nin,
            string? phoneNumber,
            DateOnly? BirthDate)
        {
            Name = (name ==null )? Name : name;
            Email =(email==null )? Email : email;
            BloodType = (bloodType == null) ? BloodType : bloodType;
            LastDonationDate = ( lastDonationDate == null) ? LastDonationDate : lastDonationDate;
            Address = (address==null )? Address : address;
            NIN = (nin==null )? NIN : nin;
            PhoneNumber = (phoneNumber== null) ? PhoneNumber : phoneNumber;
            DateOfBirth = (BirthDate==null )? DateOfBirth : BirthDate;
        }
    }
}
