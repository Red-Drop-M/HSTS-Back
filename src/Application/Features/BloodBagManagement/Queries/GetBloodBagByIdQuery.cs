using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.BloodBagManagement.Queries
{
    public record GetBloodBagByIdQuery(Guid Id) : IRequest<(BloodBagDTO? bloodBag, BaseException? err)>;
} 
