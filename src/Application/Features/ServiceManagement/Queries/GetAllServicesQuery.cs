using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.ServiceManagement.Queries
{
        public record GetAllServicesQuery(
            int Page,
            int PageSize) : IRequest<(List<ServiceDTO>? services, BaseException? err)>;
    
}
