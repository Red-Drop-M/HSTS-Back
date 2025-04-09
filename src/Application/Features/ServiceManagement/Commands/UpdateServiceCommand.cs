using MediatR;

namespace Application.Features.ServiceManagement.Commands
{
    public record UpdateServiceCommand(Guid Id, string Name) : IRequest<bool>;
}
