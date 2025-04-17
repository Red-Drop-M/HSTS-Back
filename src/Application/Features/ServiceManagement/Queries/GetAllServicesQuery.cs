using Domain.Entities;
using MediatR;
using Application.Common.Models;

namespace Application.Features.ServiceManagement.Queries
{
    public record GetAllServicesQuery : IRequest<Result<List<Service?>>>;
}
