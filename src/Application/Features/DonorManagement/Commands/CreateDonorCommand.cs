using MediatR;
using Domain.ValueObjects;
using Application.DTOs;

namespace Application.Features.DonorManagement.Commands
{
    public class CreateDonorCommand : IRequest<DonorDTO>
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public BloodType BloodType { get; set; } = BloodType.APositive();
        public DateOnly LastDonationDate { get; set; } 
        public string Address { get; set; } = string.Empty;
        public string NIN { get; set; } = string.Empty; 
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }

        public CreateDonorCommand(
            string name,
            string email,
            BloodType bloodType,
            DateOnly? lastDonationDate = null,
            string? address = null,
            string? nin  = null, // National ID Number
            string? phoneNumber = null,
            DateOnly? dateOfBirth = null)
        {
            Name = name;
            Email = email;
            BloodType = bloodType ?? BloodType.APositive(); // Default to A+ if not provided
            LastDonationDate = lastDonationDate ?? DateOnly.FromDateTime(DateTime.Now); // Default to current date
            Address = address ?? string.Empty;
            NIN = nin ?? string.Empty; // National ID Number
            PhoneNumber = phoneNumber ?? string.Empty;
            DateOfBirth = dateOfBirth ?? DateOnly.FromDateTime(DateTime.Now); // Default to current date
        }
    }

    
}