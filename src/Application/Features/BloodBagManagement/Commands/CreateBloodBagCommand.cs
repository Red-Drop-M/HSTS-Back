using MediatR;
using Domain.ValueObjects;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.BloodBagManagement.Commands
{
    public class CreateBloodBagCommand : IRequest<(BloodBagDTO? bloodBag, BaseException? err)>
    {
        public BloodBagType BloodBagType { get;  } 
        public BloodType BloodType { get; }  
        public BloodBagStatus Status { get; } = BloodBagStatus.Ready();
        public DateOnly? ExpirationDate { get;  }
        public DateOnly? AcquiredDate { get;  } = DateOnly.FromDateTime(DateTime.Now);
        public Guid DonorId { get;  }
        public Guid? RequestId { get; } 

        public CreateBloodBagCommand(BloodType bloodType, BloodBagType bloodBagType, DateOnly? expirationDate,DateOnly? acquiredDate, Guid donorId, Guid? requestId)  
        {
            BloodBagType = bloodBagType;
            BloodType = bloodType;
            Status = BloodBagStatus.Ready();
            ExpirationDate = expirationDate;
            AcquiredDate = acquiredDate ?? DateOnly.FromDateTime(DateTime.Now);
            DonorId = donorId;
            RequestId = requestId;
        }
    }
}