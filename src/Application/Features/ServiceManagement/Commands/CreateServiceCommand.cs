using MediatR;
using Application.Common.Models;
using System;

namespace Application.Features.ServiceManagement.Commands
{
    public record CreateServiceCommand(string Name) : IRequest<Result<Guid>>;
}
