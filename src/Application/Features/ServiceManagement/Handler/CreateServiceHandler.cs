using Domain.Entities;
using Domain.Repositories;
using MediatR;
using Application.Features.ServiceManagement.Commands;
using Application.DTOs;

namespace Application.Features.ServiceManagement.Handler
{
    public class CreateServiceHandler : IRequestHandler<CreateServiceCommand, ServiceDTO>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<CreateServiceHandler> _logger;

        public CreateServiceHandler(IServiceRepository serviceRepository, ILogger<CreateServiceHandler> logger)
        {
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<ServiceDTO> Handle(CreateServiceCommand Service, CancellationToken cancellationToken)
        {
            try
            {
                var newService = new Service(Service.Name);
                await _serviceRepository.AddAsync(newService);
                return new ServiceDTO
                {
                    Id = newService.Id,
                    Name = newService.Name,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                throw;
            }
        }
    }
}
