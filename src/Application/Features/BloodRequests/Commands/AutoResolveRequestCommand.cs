using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.BloodRequests.Commands
{
    public class AutoResolveRequestCommand : IRequest<(RequestDto? request, BaseException? err)>
    {
        public Guid Id { get; }

        public AutoResolveRequestCommand(Guid id)
        {
            Id = id;
        }
    }
}