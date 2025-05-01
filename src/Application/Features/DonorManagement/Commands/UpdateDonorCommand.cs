using MediatR;
using Application.DTOs;
using Shared.Exceptions;
using Domain.ValueObjects;

namespace Application.Features.DonorManagement.Commands
{
    public class UpdateDonorCommand : IRequest<(DonorDTO? donor, BaseException? err)>
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public BloodType? BloodType { get; set; } = BloodType.APositive();
        public DateOnly? LastDonationDate { get; set; } 
        public string? Address { get; set; } = string.Empty;
        public string? NIN { get; set; } = string.Empty; 
        public string? PhoneNumber { get; set; } = string.Empty;
        public DateOnly? BirthDate { get; set;} = null; 

        public UpdateDonorCommand(
            Guid id,
            string? name,
            string? email,
            string? phoneNumber,
            string? address ,
            BloodType? bloodType ,
            DateOnly? lastDonationDate ,
            string? nin ,
            DateOnly? birthDate )
        {
            Id = id;
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address ;
            BloodType = bloodType ;
            LastDonationDate = lastDonationDate ;
            NIN = nin;
            BirthDate = birthDate;
        }

    }
} 