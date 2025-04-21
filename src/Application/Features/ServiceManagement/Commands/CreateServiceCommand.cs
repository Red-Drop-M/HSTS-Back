using MediatR;
using Application.DTOs;

namespace Application.Features.ServiceManagement.Commands
{
    public class CreateServiceCommand : IRequest<ServiceDTO>
    {
        public string Name { get; }  
    }
}