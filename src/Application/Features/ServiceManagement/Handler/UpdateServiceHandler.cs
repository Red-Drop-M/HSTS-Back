using Domain.Repositories;
using MediatR;
using Application.DTOs;
using Application.Features.ServiceManagement.Commands;
using Shared.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Features.ServiceManagement.Handler
{
 
    public class UpdateServiceHandler : IRequest<(ServiceDTO? service, BaseException? err)>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<UpdateServiceHandler> _logger;

        public UpdateServiceHandler(IServiceRepository serviceRepository, ILogger<UpdateServiceHandler> logger)
        {
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<(ServiceDTO? service, BaseException? err)> Handle(UpdateServiceCommand Service, CancellationToken ct)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(Service.Id);
                if (service == null)
                {
                    _logger.LogError("Service not found");
                    throw new NotFoundException("No service found with the provided ID", "Updating service");
                }

                service.Update(Service.Name);
                await _serviceRepository.UpdateAsync(service);
                _logger.LogInformation("Service updated successfully");

                var serviceDto = new ServiceDTO
                {
                    Id = service.Id,
                    Name = service.Name
                };

                return (serviceDto, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError("Error while updating service: {Message}", ex.Message);
                return (null, ex);
            }
        }
    }

}
