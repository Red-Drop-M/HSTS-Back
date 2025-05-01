using Domain.Repositories;
using MediatR;
using Application.DTOs;
using Application.Features.ServiceManagement.Commands;
using Shared.Exceptions;


namespace Application.Features.ServiceManagement.Handler
{
    public class DeleteServiceHandler : IRequestHandler<DeleteServiceCommand, (ServiceDTO? service, BaseException? err)>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<DeleteServiceHandler> _logger;

        public DeleteServiceHandler(IServiceRepository serviceRepository, ILogger<DeleteServiceHandler> logger)
        {
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<(ServiceDTO? service, BaseException? err)> Handle(
            DeleteServiceCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(command.Id);
                if (service == null)
                {
                    _logger.LogError("Service with ID {ServiceId} not found", command.Id);
                    return (null, new NotFoundException($"Service {command.Id} not found", "delete service"));
                }

                await _serviceRepository.DeleteAsync(service.Id);

                var serviceDto = new ServiceDTO
                {
                    Id = service.Id,
                    Name = service.Name,
                };

                return (serviceDto, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Failed to delete service {ServiceId}", command.Id);
                return (null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting service {ServiceId}", command.Id);
                return (null, new InternalServerException("Failed to delete service", "delete service"));
            }
        }
    }
}