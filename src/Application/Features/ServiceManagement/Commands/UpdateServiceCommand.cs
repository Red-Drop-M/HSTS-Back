using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.ServiceManagement.Commands
{
    public class UpdateServiceCommand : IRequest<(ServiceDTO? service, BaseException? err)>
    {
        public Guid Id { get; }
        public string Name { get; } 
        public UpdateServiceCommand(Guid id,  string name)
        {
            Id = id;
            Name = name;

        }
    }

}
