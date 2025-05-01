using Domain.Repositories;
using MediatR;
using Domain.ValueObjects;
using Application.Features.ServiceManagement.Queries;
using Application.DTOs;
using Shared.Exceptions;



namespace Application.Features.ServiceManagement.Handler
{
    public class GetAllServicesHandler : IRequestHandler<GetAllServicesQuery, (List<ServiceDTO>? services,  BaseException? err)>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<GetAllServicesHandler> _logger;

        public GetAllServicesHandler(IServiceRepository serviceRepository, ILogger<GetAllServicesHandler> logger)
        {
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<(List<ServiceDTO>? services, BaseException? err)> Handle(GetAllServicesQuery Service, CancellationToken cancellationToken)
        {
            try
            {

                var services = await _serviceRepository.GetServicesAsync();
                if (services == null || services.Count == 0)
                {
                    _logger.LogWarning("No services found");
                    return (null,  new NotFoundException("No services found", "Fetching services"));
                }
                var serviceDtos = services.Select(s => new ServiceDTO
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList();
                return (serviceDtos, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error fetching services");
                return (null,  ex);
            }
        }
    }
}
