using Domain.Repositories;
using MediatR;
using Application.Features.ServiceManagement.Queries;
using Application.DTOs;
using Shared.Exceptions;


namespace Application.Features.ServiceManagement.Handler
{
    public class GetServiceByIdHandler : IRequestHandler<GetServiceByIdQuery, (ServiceDTO? service, BaseException? err)>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ILogger<GetServiceByIdHandler> _logger;

        public GetServiceByIdHandler(IServiceRepository serviceRepository, ILogger<GetServiceByIdHandler> logger)
        {
            _serviceRepository = serviceRepository;
            _logger = logger;
        }

        public async Task<(ServiceDTO? service, BaseException? err)> Handle(GetServiceByIdQuery Service, CancellationToken ct)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(Service.Id);
                if (service == null)
                {
                    _logger.LogError("Service not found");
                    return (null, new NotFoundException("Service not found", "get service"));
                }
                var serviceDto = new ServiceDTO
                {
                    Id = service.Id,
                    Name = service.Name
                };
                return (serviceDto, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex.Message, "An internal exception occurred");
                return (null, ex);
            }
        }
    }
}
