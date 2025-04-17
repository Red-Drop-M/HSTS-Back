using MediatR;
using Application.Common.Models;
using Domain.ValueObjects;

namespace Application.Features.BloodBagManagement.Commands
{
    public class UpdateBloodBagCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
        public BloodType BloodType { get; set; } = BloodType.APositive();
        public BloodBagType BloodBagType { get; set; } = BloodBagType.Blood();
        public DateOnly ExpirationDonorDate { get; set; }
        public Guid DonorId { get; set; }
        public Guid? RequestId { get; set; }
    }
} 