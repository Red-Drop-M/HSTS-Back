using Domain.Entities;
using Domain.Repositories;
using MediatR;
using Application.DTOs;
using Infrastructure.ExternalServices.Kafka;
using Application.Features.BloodRequests.Commands;
using Microsoft.Extensions.Options;
using Domain.Events;
using Shared.Exceptions;
using FastEndpoints;
using Application.Interfaces;

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
                _logger.LogInformation("new id: {RequestId}", newRequest.Id);
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

                await new AutoReuqestResolverEvent(newRequest).PublishAsync(Mode.WaitForNone); // Create and publish the event
                _logger.LogInformation("Request created successfully");

                // Return the DTO
                return new RequestDto
                {
                    Id = newRequest.Id,
                    Priority = newRequest.Priority.Value,
                    BloodType = newRequest.BloodType.Value,
                    BloodBagType = newRequest.BloodBagType.Value,
                    RequestDate = newRequest.RequestDate,
                    DueDate = newRequest.DueDate,
                    Status = newRequest.Status.Value,
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