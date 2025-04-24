using MediatR;
using Domain.ValueObjects;
using Shared.Exceptions;
using Application.DTOs;

namespace Application.Features.BloodBagManagement.Queries
{
    public record GetAllBloodBagsQuery(
        int PageNumber,
        int PageSize,
        BloodBagType? BloodBagType,
        BloodType? BloodType,
        BloodBagStatus? Status,
        DateOnly? ExpirationDate,
        DateOnly? AcquiredDate,
        Guid? DonorId,
        Guid? RequestId) 
        : IRequest<(List<BloodBagDTO>? bloodBags, int? total, BaseException? err)>;
} 
