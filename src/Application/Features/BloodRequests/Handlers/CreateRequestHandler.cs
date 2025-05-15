using Domain.Entities;
using Domain.Repositories;
using MediatR;
using Application.DTOs;
using Application.interfaces;
using Infrastructure.ExternalServices.Kafka;
using Application.Features.BloodRequests.Commands;
using Microsoft.Extensions.Options;
using Domain.Events;
using Microsoft.Extensions.FileProviders;
using Shared.Exceptions;

namespace Application.Features.BloodRequests.Handlers
{
    public class CreateRequestHandler : IRequestHandler<CreateRequestCommand, RequestDto>
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IEventProducer _eventProducer;
        private readonly IServiceRepository _serviceRepository;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly ILogger<CreateRequestHandler> _logger;

        public CreateRequestHandler(
            IRequestRepository requestRepository,
            ILogger<CreateRequestHandler> logger,
            IEventProducer eventProducer,
            IOptions<KafkaSettings> kafkaSettings,
            IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
            _eventProducer = eventProducer;
            _kafkaSettings = kafkaSettings;
            _requestRepository = requestRepository;
            _logger = logger;
        }

        public async Task<RequestDto> Handle(CreateRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Create a new request
                var newRequest = new Request(
                    request.BloodType,
                    request.Priority,
                    request.BloodBagType,
                    request.RequestDate,
                    request.DueDate,
                    request.RequestStatus,
                    request.MoreDetails,
                    request.RequiredQty,
                    request.AquiredQty,
                    request.ServiceId,
                    request.DonorId);

                // Save the request to the database
                await _requestRepository.AddAsync(newRequest);

                // Check if the service exists
                if (!newRequest.ServiceId.HasValue)
                {
                    _logger.LogError("ServiceId is null");
                    throw new ArgumentException("ServiceId cannot be null", nameof(request.ServiceId));
                }

                var service = await _serviceRepository.GetByIdAsync(newRequest.ServiceId.Value);
                if (service == null)
                {
                    _logger.LogError("Service not found");
                    throw new NotFoundException("Service not found", "CreateRequestHandler");
                }

                // Create and publish the event
                var requestCreatedEvent = new RequestCreatedEvent(
                    newRequest.Id,
                    newRequest.BloodType,
                    newRequest.Priority,
                    newRequest.BloodBagType,
                    newRequest.RequestDate,
                    newRequest.DueDate,
                    newRequest.Status,
                    newRequest.MoreDetails,
                    newRequest.RequiredQty,
                    newRequest.AquiredQty,
                    service.Name // Pass the service name to the event
                );

                var topic = _kafkaSettings.Value.Topics["blood-request-created"];
                await _eventProducer.ProduceAsync(requestCreatedEvent,topic);

                _logger.LogInformation("Request created successfully");

                // Return the DTO
                return new RequestDto
                {
                    Id = newRequest.Id,
                    Priority = newRequest.Priority,
                    BloodType = newRequest.BloodType,
                    BloodBagType = newRequest.BloodBagType,
                    RequestDate = newRequest.RequestDate,
                    DueDate = newRequest.DueDate,
                    Status = newRequest.Status,
                    MoreDetails = newRequest.MoreDetails,
                    RequiredQty = newRequest.RequiredQty,
                    AquiredQty = newRequest.AquiredQty,
                    ServiceId = newRequest.ServiceId,
                    DonorId = newRequest.DonorId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating request");
                throw;
            }
        }
    }
}