using MediatR;
using Application.DTOs;
using Shared.Exceptions;
using Domain.ValueObjects;

namespace Application.Features.DonorManagement.Commands
{
    public class UpdateDonorCommand : IRequest<(DonorDTO? donor, BaseException? err)>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set;}
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string NIN { get; set; } = string.Empty; // National ID Number
        public BloodType BloodType { get; set; } = BloodType.APositive();
        public DateOnly LastDonationDate { get; set; } 

        public UpdateDonorCommand(
            Guid id,
            string name,
            DateOnly birthDate,
            string phoneNumber,
            string email,
            string address,
            string? nin = null, // National ID Number
            BloodType? bloodType = null,
            DateOnly? lastDonationDate = null)
        {
            Id = id;
            Name = name;
            BirthDate = birthDate;
            PhoneNumber = phoneNumber;
            Email = email;
            Address = address;
            NIN = nin ?? string.Empty; // National ID Number
            BloodType = bloodType ?? BloodType.APositive(); // Default to A+ if not provided
            LastDonationDate = lastDonationDate ?? DateOnly.FromDateTime(DateTime.Now); // Default to current date
        }

    }
} 