using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.ServiceManagement.Queries
{
    public record GetServiceByIdQuery(Guid Id) : IRequest<(ServiceDTO? service, BaseException? err)>;
}
