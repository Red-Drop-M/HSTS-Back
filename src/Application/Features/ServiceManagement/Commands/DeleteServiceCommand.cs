using MediatR;
using Application.Common.Models;

namespace Application.Features.ServiceManagement.Commands
{
    public record DeleteServiceCommand(Guid Id) : IRequest<Result<bool>>;
}
