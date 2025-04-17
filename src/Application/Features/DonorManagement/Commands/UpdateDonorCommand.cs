using MediatR;
using Application.Common.Models;
using Domain.ValueObjects;

namespace Application.Features.DonorManagement.Commands
{
    public class UpdateDonorCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public BloodType BloodType { get; set; } = BloodType.APositive();
        public string Email { get; set; }       = string.Empty;
        public DateOnly DateOfBirth { get; set; } 
        public string NIN { get; set; }       = string.Empty;
        public string PhoneNumber { get; set; }     = string.Empty;
        public string Address { get; set; }     = string.Empty;
        public DateOnly LastDonationDate { get; set; } 
    }
} 