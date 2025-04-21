using Application.DTOs;
using Shared.Exceptions;
using MediatR;


namespace Application.Features.BloodBagManagement.Commands
{
    public record DeleteBloodBagCommand(Guid Id) : IRequest<(BloodBagDTO? bloodBag, BaseException? err)>; 
} 
