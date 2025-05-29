using MediatR;
using Domain.Repositories;
using Application.Features.BloodBagManagement.Commands;
using Application.DTOs;
using Shared.Exceptions;
using Application.Interfaces;
using Microsoft.Extensions.Options;
using Infrastructure.ExternalServices.Kafka;
using Domain.Events;
using System.Text.Json;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class DeleteBloodBagHandler : IRequestHandler<DeleteBloodBagCommand, (BloodBagDTO? BloodBag, BaseException? err)>
    {
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<DeleteBloodBagHandler> _logger;
        private readonly IEventProducer _eventProducer;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly IGlobalStockRepository _globalStockRepository;

        public DeleteBloodBagHandler(
            IBloodBagRepository bloodBagRepository, 
            IRequestRepository requestRepository,
            ILogger<DeleteBloodBagHandler> logger,
            IEventProducer eventProducer,
            IOptions<KafkaSettings> kafkaSettings,
            IGlobalStockRepository globalStockRepository)
        {
            _bloodBagRepository = bloodBagRepository;
            _requestRepository = requestRepository;
            _logger = logger;
            _eventProducer = eventProducer;
            _kafkaSettings = kafkaSettings;
            _globalStockRepository = globalStockRepository;
        }

        public async Task<(BloodBagDTO? BloodBag, BaseException? err)> Handle(DeleteBloodBagCommand command, CancellationToken cancellationToken)
        {
            try 
            {
                var bloodBag = await _bloodBagRepository.GetByIdAsync(command.Id);
                if (bloodBag == null)
                {
                    _logger.LogError("Blood bag with ID {BloodBagId} not found", command.Id);
                    return (null, new NotFoundException($"Blood bag {command.Id} not found", "delete blood bag"));
                }

                // If blood bag is associated with a request, update the request
                if (bloodBag.RequestId.HasValue)
                {
                    var request = await _requestRepository.GetByIdAsync(bloodBag.RequestId.Value);
                    if (request != null)
                    {
                        // Decrement the acquired quantity
                        if (request.AquiredQty > 0)
                        {
                            request.UpdateAcquiredQuantity(request.AquiredQty - 1);
                            await _requestRepository.UpdateAsync(request);
                            _logger.LogInformation("Decremented acquired quantity for request {RequestId}", request.Id);
                            
                            // Create and produce Kafka event for the request update
                            var topic = _kafkaSettings.Value.Topics["UpdateRequest"];
                            var updateRequestEvent = new UpdateRequestEvent(
                                requestId: request.Id,
                                priority: null,
                                status: null,
                                acquiredQty: request.AquiredQty,
                                requiredQty: request.RequiredQty,
                                dueDate: null
                            );
                            
                            var message = JsonSerializer.Serialize(updateRequestEvent);
                            await _eventProducer.ProduceAsync(topic, message);
                            _logger.LogInformation("Produced UpdateRequest event for request {RequestId} with new acquired quantity {AcquiredQty}", 
                                request.Id, request.AquiredQty);
                        }
                    }
                }

                // Update global stock based on blood bag status
                var globalStock = await _globalStockRepository.GetByKeyAsync(bloodBag.BloodType, bloodBag.BloodBagType);
                if (globalStock != null)
                {
                    if (bloodBag.Status.Value == "aquired")
                    {
                        globalStock.DecrementAcquiredCount(1);
                        _logger.LogInformation("Decremented acquired count for {BloodType} {BloodBagType}", 
                            bloodBag.BloodType.Value, bloodBag.BloodBagType.Value);
                    }
                    else if (bloodBag.Status.Value == "ready")
                    {
                        globalStock.DecrementAvailableCount(1);
                        _logger.LogInformation("Decremented ready count for {BloodType} {BloodBagType}", 
                            bloodBag.BloodType.Value, bloodBag.BloodBagType.Value);
                    }
                    
                    await _globalStockRepository.UpdateAsync(globalStock);
                }

                // Mark as deleted instead of physically deleting
                bloodBag.MarkAsDeleted();
                await _bloodBagRepository.UpdateAsync(bloodBag);
                _logger.LogInformation("Blood bag {BloodBagId} marked as deleted", bloodBag.Id);

                var bloodBagDto = new BloodBagDTO
                {
                    Id = bloodBag.Id,
                    BloodBagType = bloodBag.BloodBagType,
                    BloodType = bloodBag.BloodType,
                    Status = bloodBag.Status,
                    ExpirationDate = bloodBag.ExpirationDate,
                    AcquiredDate = bloodBag.AcquiredDate,
                    DonorId = bloodBag.DonorId ?? Guid.Empty,
                    RequestId = bloodBag.RequestId,
                    IsDeleted = true
                };

                return (bloodBagDto, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Failed to delete blood bag {BloodBagId}", command.Id);
                return (null, ex);
            }
        }
    }
}