using Domain.Entities;
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

/*
using Domain.Entities;
using Domain.ValueObjects;



namespace Application.DTOs
{
    public class RequestDto
    {
        public Guid Id { get; set; }
        public Priority Priority { get; set; } = Priority.Standard();
        public BloodType BloodGroup { get; set; } = BloodType.APositive();
        public BloodBagType BloodBagType { get; set; } = BloodBagType.Blood();
        public DateOnly RequestDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending();
        public string? MoreDetails { get; set; }
        public int RequiredQty { get; set; }
        public int AquiredQty { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? DonorId { get; set; }
    }
}




*/