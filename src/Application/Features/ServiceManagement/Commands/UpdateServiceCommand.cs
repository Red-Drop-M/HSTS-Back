using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.ServiceManagement.Commands
{
    public class UpdateServiceCommand : IRequest<(ServiceDTO? service, BaseException? err)>
    {
        public Guid Id { get; set; }
        public string Name { get; } = string.Empty;

        public UpdateServiceCommand(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
        public UpdateServiceCommand(string name)
        {
            Name = name;
        }
    }
        

}
