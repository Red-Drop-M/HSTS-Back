using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.ServiceManagement.Commands
{
    public record DeleteServiceCommand(Guid Id) : IRequest<(ServiceDTO? service, BaseException? err)>;
}
