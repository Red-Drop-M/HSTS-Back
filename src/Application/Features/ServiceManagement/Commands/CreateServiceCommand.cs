using MediatR;
using Application.Common.Models;

namespace Application.Features.ServiceManagement.Commands
{
    public record CreateServiceCommand(Guid id,string Name) : IRequest<Result<Guid>>;
}
