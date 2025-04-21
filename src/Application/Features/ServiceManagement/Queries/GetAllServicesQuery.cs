using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.ServiceManagement.Queries
{
        public record GetAllServicesQuery(
            int Page,
            int PageSize,
            string? Name,
            string? Description) : IRequest<(List<ServiceDTO>? services, int? total, BaseException? err)>;
    
}
