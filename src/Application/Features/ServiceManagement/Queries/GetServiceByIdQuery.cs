using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.ServiceManagement.Queries
{
    public record GetServiceByIdQuery(Guid Id) : IRequest<Result<Service?>>;
}
