using MediatR;
using Domain.ValueObjects;
using Shared.Exceptions;
using Application.DTOs;

namespace Application.Features.BloodBagManagement.Commands
{
    public class UpdateBloodBagCommand : IRequest<(BloodBagDTO? bloodBag, BaseException? err)>
    {
        public Guid Id { get; }
        public BloodType? BloodType { get; } 
        public BloodBagType? BloodBagType { get; } 
        public DateOnly? ExpirationDate { get; } 

        public DateOnly? AcquiredDate { get; } = DateOnly.FromDateTime(DateTime.Now);
        public Guid DonorId { get; } 
        public Guid? RequestId { get; } 

        public UpdateBloodBagCommand(
            Guid id,
            BloodType? bloodType = null,
            BloodBagType? bloodBagType = null,
            DateOnly? expirationDate = null,
            DateOnly? acquiredDate = null,
            Guid donorId = default,
            Guid? requestId = null)
        {
            Id = id;
            BloodType = bloodType;
            BloodBagType = bloodBagType;
            ExpirationDate = expirationDate;
            AcquiredDate = acquiredDate;
            DonorId = donorId;
            RequestId = requestId;
        }
    }
} 
