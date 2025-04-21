using MediatR;
using Domain.ValueObjects;
using Application.DTOs;

namespace Application.Features.BloodBagManagement.Commands
{
    public class CreateBloodBagCommand : IRequest<BloodBagDTO>
    {
        public BloodType BloodType { get; }  
        public BloodBagType BloodBagType { get;  } 
        public DateOnly ExpirationDate { get;  }
        public Guid DonorId { get;  }
        public Guid? RequestId { get; } 

    }
} 