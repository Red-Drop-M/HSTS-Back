using MediatR;

namespace Application.Features.ServiceManagement.Commands
{
    public record DeleteServiceCommand(Guid Id) : IRequest<bool>;
}
