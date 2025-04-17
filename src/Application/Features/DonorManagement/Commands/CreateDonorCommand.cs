using MediatR;
using Application.Common.Models;
using Domain.ValueObjects;

namespace Application.Features.DonorManagement.Commands
{
    public class CreateDonorCommand : IRequest<Result<Guid>>
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public DateOnly DateOfBirth { get; private set; }
        public BloodType BloodType { get; private set; } = BloodType.APositive();
        public string NIN { get; private set; } = string.Empty; 
        public string PhoneNumber { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public DateOnly LastDonationDate { get; private set; }
    }
} 