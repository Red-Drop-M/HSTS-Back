using MediatR;
using Domain.ValueObjects;
using Shared.Exceptions;
using Application.DTOs;

namespace Application.Features.BloodBagManagement.Commands
{
    public class UpdateBloodBagCommand : IRequest<(BloodBagDTO? bloodBag, BaseException? err)>
    {
        public Guid Id { get; }
        public BloodBagType? BloodBagType { get; } 
        public BloodType? BloodType { get; } 
        public BloodBagStatus? Status { get; }
        public DateOnly? ExpirationDate { get; } 
        public DateOnly? AcquiredDate { get; } = DateOnly.FromDateTime(DateTime.Now);
        public Guid? RequestId { get; } 

        public UpdateBloodBagCommand(
            Guid id,
            BloodBagType? bloodBagType = null,
            BloodType? bloodType = null,
            BloodBagStatus? status = null,
            DateOnly? expirationDate = null,
            DateOnly? acquiredDate = null,
            Guid? requestId = null)
        {
            Id = id;
            BloodBagType = bloodBagType;
            BloodType = bloodType;
            Status = status;
            ExpirationDate = expirationDate;
            AcquiredDate = acquiredDate;
            RequestId = requestId;
        }
    }
} 
