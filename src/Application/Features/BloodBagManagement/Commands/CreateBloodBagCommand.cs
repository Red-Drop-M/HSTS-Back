using MediatR;
using Application.Common.Models;
using Domain.ValueObjects;

namespace Application.Features.BloodBagManagement.Commands
{
    public class CreateBloodBagCommand : IRequest<Result<Guid>>
    {
        public BloodType BloodType { get; set; } = BloodType.APositive();
        public BloodBagType BloodBagType { get; set; } = BloodBagType.Blood();
        public DateOnly ExpirationDonorDate { get; set; }
        public Guid DonorId { get; set; }
        public Guid? RequestId { get; set; } 


    }
} 