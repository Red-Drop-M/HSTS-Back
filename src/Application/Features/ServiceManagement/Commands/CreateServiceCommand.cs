using MediatR;
using Shared.Exceptions;
using Application.DTOs;

namespace Application.Features.ServiceManagement.Commands
{
    public class CreateServiceCommand : IRequest<(ServiceDTO? service, BaseException? err)> 
    {
        public string Name { get; }  
    }
}