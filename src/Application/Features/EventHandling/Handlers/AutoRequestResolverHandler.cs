using FastEndpoints;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Domain.Events;
using Shared.Exceptions;
using Application.Interfaces;
using Infrastructure.ExternalServices.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MediatR;
namespace Application.Features.EventHandling.Handlers
{
    public class AutoRequestResolverHandler : IEventHandler<AutoReuqestResolverEvent>
    {
        private readonly IRequestRepository _requestRepository;
     
        private readonly IEventProducer _eventProducer;
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly ILogger<AutoRequestResolverHandler> _logger;
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        public AutoRequestResolverHandler(IRequestRepository requestRepository, IEventProducer eventProducer, IOptions<KafkaSettings> kafkaSettings, ILogger<AutoRequestResolverHandler> logger, IGlobalStockRepository globalStockRepository, IBloodBagRepository bloodBagRepository,  IBackgroundTaskQueue backgroundTaskQueue)
        {
           
            _bloodBagRepository = bloodBagRepository;
            _eventProducer = eventProducer;
            _kafkaSettings = kafkaSettings;
            _requestRepository = requestRepository;
            _logger = logger;
            _globalStockRepository = globalStockRepository;
            _backgroundTaskQueue = backgroundTaskQueue;
        }
        public async Task HandleAsync(AutoReuqestResolverEvent request, CancellationToken ct)
        {
            // Queue the work to be done in a background thread with its own scope
            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (scope, token) => 
            {
                // Get new instances of dependencies from the scope
                var requestRepository = scope.ServiceProvider.GetRequiredService<IRequestRepository>();
                var globalStockRepository = scope.ServiceProvider.GetRequiredService<IGlobalStockRepository>();
                var bloodBagRepository = scope.ServiceProvider.GetRequiredService<IBloodBagRepository>();
              
                var eventProducer = scope.ServiceProvider.GetRequiredService<IEventProducer>();
                var kafkaSettings = scope.ServiceProvider.GetRequiredService<IOptions<KafkaSettings>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<AutoRequestResolverHandler>>();
                
                // Process with fresh dependencies
                try
                {
                    var stock = await globalStockRepository.GetByKeyAsync(request.Request.BloodType, request.Request.BloodBagType);
                    if (stock == null)
                    {
                        logger.LogError("no stock found for the request");
                        throw new NotFoundException("no stock found for the request", "AutoRequestResolverEvent");
                    }
                    if (stock.ReadyCount - request.Request.RequiredQty < stock.MinStock)
                    {
                        logger.LogError("not enough stock available checking for critical stock");
                        if ((stock.ReadyCount - request.Request.RequiredQty >= stock.CriticalStock) && request.Request.Priority.Value == Priority.Critical().Value)
                        {
                            var filters = new BloodBagFilter
                            {
                                BloodType = request.Request.BloodType,
                                BloodBagType = request.Request.BloodBagType,
                                Status = BloodBagStatus.Ready(),
                                RequestId = null,
                                ExpirationDate = null,
                                AcquiredDate = null,
                                CreatedAt = null,
                                UpdatedAt = null
                            };
                            var (bloodBags, Count) = await bloodBagRepository.GetAllAsync(1, request.Request.RequiredQty, filters);
                            foreach (var bloodBag in bloodBags)
                            {
                                bloodBag.UseBloodBag(request.Request.Id);
                                stock.UpdateCounts(stock.CountExpired, stock.CountExpiring, stock.ReadyCount - Count);
                                await bloodBagRepository.UpdateAsync(bloodBag);
                                await globalStockRepository.UpdateAsync(stock);
                                request.Request.UpdateAquiredQty();
                                request.Request.BloodSacs.Add(bloodBag);
                            }
                            request.Request.Resolve();
                            await requestRepository.UpdateAsync(request.Request);
                            logger.LogInformation("request resolved successfully");
                            logger.LogInformation($"Assigned {bloodBags.Count} blood bags to critical priority request {request.Request.Id}");
                        }
                        else
                        {
                            logger.LogInformation("not enough stock available for the request");
                            try {
                                // Get topic value from settings using the correct key
                                var topic = kafkaSettings.Value.Topics["BloodRequests"];  // This should match a key in your appsettings.json
                                logger.LogInformation("Using Kafka topic: '{Topic}'", topic);
                                
                                if (string.IsNullOrEmpty(topic))
                                {
                                    logger.LogError("Topic name is empty for key 'BloodRequests'");
                                    return;
                                }
                                
                                if (request.Request.ServiceId == null)
                                {
                                    logger.LogError("ServiceId is null");
                                    throw new NotFoundException("ServiceId is null", "AutoRequestResolverEvent");
                                }
                                var Service = request.Request.Service;
                                if (Service == null)
                                {
                                    logger.LogError("Service not found");
                                    throw new NotFoundException("Service not found", "AutoRequestResolverEvent");
                                }
                                var @event = new RequestCreatedEvent(
                                    request.Request.Id,
                                    request.Request.BloodType,
                                    request.Request.Priority,
                                    request.Request.BloodBagType,
                                    request.Request.RequestDate,
                                    request.Request.DueDate,
                                    request.Request.Status,
                                    request.Request.MoreDetails,
                                    request.Request.RequiredQty,
                                    request.Request.AquiredQty,
                                    Service.Name);
                                await eventProducer.ProduceAsync(topic, JsonSerializer.Serialize(@event));
                            } 
                            catch (KeyNotFoundException ex) {
                                logger.LogError(ex, "Topic key 'BloodRequests' not found in configuration");
                            }
                            catch (Exception ex) {
                                logger.LogError(ex, "Error publishing to Kafka");
                            }
                        }
                    }
                    var filtersNormal = new BloodBagFilter
                    {
                        BloodType = request.Request.BloodType,
                        BloodBagType = request.Request.BloodBagType,
                        Status = BloodBagStatus.Ready(),
                        RequestId = null,
                        ExpirationDate = null,
                        AcquiredDate = null,
                        CreatedAt = null,
                        UpdatedAt = null
                    };
                    var (bloodBagsNormal, countNormal) = await bloodBagRepository.GetAllAsync(1, request.Request.RequiredQty, filtersNormal);
                    foreach (var bloodBag in bloodBagsNormal)
                    {
                        bloodBag.UseBloodBag(request.Request.Id);
                        stock.UpdateCounts(stock.CountExpired, stock.CountExpiring, stock.ReadyCount - countNormal);
                        request.Request.BloodSacs.Add(bloodBag);
                        request.Request.UpdateAquiredQty();
                        await bloodBagRepository.UpdateAsync(bloodBag);
                        await globalStockRepository.UpdateAsync(stock);
                    }
                    request.Request.Resolve();
                    await requestRepository.UpdateAsync(request.Request);
                    logger.LogInformation("request resolved successfully");
                    logger.LogInformation($"Assigned {bloodBagsNormal.Count} blood bags to critical priority request {request.Request.Id}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while handling AutoRequestResolverEvent");
                }
            });
        }
    }    
}