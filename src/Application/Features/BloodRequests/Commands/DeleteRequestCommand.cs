using Application.DTOs;
using Shared.Exceptions;
using MediatR;
namespace Application.Features.BloodRequests.Commands
{
    public record DeleteRequestCommand(Guid Id) : IRequest<(RequestDto? request, BaseException? err)>; // Use the correct type for the response
}
