using Domain.Entities;
using MediatR;

namespace Application.Features.ServiceManagement.Queries
{
    public record GetAllServicesQuery : IRequest<List<Service?>>;
}
