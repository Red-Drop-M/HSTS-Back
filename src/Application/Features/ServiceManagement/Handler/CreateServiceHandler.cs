using Domain.Entities;
using Domain.Repositories;
using MediatR;
using Application.Features.ServiceManagement.Commands;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.ServiceManagement.Handler
{
    public class CreateServiceHandler : IRequestHandler<CreateServiceCommand, (ServiceDTO? service, BaseException? err)>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<CreateServiceHandler> _logger;

        public CreateServiceHandler(IServiceRepository serviceRepository, ILogger<CreateServiceHandler> logger)
        {
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<(ServiceDTO? service,BaseException? err)> Handle(CreateServiceCommand Service, CancellationToken cancellationToken)
        {
            try
            {
                var newService = new Service(Service.Name);
                await _serviceRepository.AddAsync(newService);
                return (new ServiceDTO
                {
                    Id = newService.Id,
                    Name = newService.Name,
                },null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error creating service");
                return (null,ex);
            }
        }
    }
}
