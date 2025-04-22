using MediatR;
using Domain.ValueObjects;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.BloodBagManagement.Commands
{
    public class CreateBloodBagCommand : IRequest<(BloodBagDTO? bloodBag, BaseException? err)>
    {
        public BloodType BloodType { get; }  
        public BloodBagType BloodBagType { get;  } 
        public DateOnly? ExpirationDate { get;  }
        public DateOnly? AcquiredDate { get;  } = DateOnly.FromDateTime(DateTime.Now);
        public Guid DonorId { get;  }
        public Guid? RequestId { get; } 

        public CreateBloodBagCommand(BloodType bloodType, BloodBagType bloodBagType, DateOnly? expirationDate,DateOnly? acquiredDate, Guid donorId, Guid? requestId)  
        {
            BloodType = bloodType;
            BloodBagType = bloodBagType;
            ExpirationDate = expirationDate;
            AcquiredDate = acquiredDate ?? DateOnly.FromDateTime(DateTime.Now);
            DonorId = donorId;
            RequestId = requestId;
        }
    }
}