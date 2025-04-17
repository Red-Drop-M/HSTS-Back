using MediatR;
using Application.Common.Models;

namespace Application.Features.ServiceManagement.Commands
{
    public class UpdateServiceCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
