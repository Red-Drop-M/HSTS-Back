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
        private readonly IServiceRepository _serviceRepository;
        private readonly IEventProducer _eventProducer;
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly ILogger<AutoRequestResolverHandler> _logger;
        private readonly IGlobalStockRepository _globalStockRepository;
        public AutoRequestResolverHandler(IRequestRepository requestRepository, IEventProducer eventProducer, IOptions<KafkaSettings> kafkaSettings, ILogger<AutoRequestResolverHandler> logger, IGlobalStockRepository globalStockRepository, IBloodBagRepository bloodBagRepository, IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
            _bloodBagRepository = bloodBagRepository;
            _eventProducer = eventProducer;
            _kafkaSettings = kafkaSettings;
            _requestRepository = requestRepository;
            _logger = logger;
            _globalStockRepository = globalStockRepository;
        }
        public async Task HandleAsync(AutoReuqestResolverEvent request, CancellationToken ct)
        {
            try
            {
                var stock = await _globalStockRepository.GetByKeyAsync(request.Request.BloodType, request.Request.BloodBagType);
                if (stock == null)
                {
                    _logger.LogError("no stock found for the request");
                    throw new NotFoundException("no stock found for the request", "AutoRequestResolverEvent");
                }
                if (stock.ReadyCount - request.Request.RequiredQty < stock.MinStock)
                {
                    _logger.LogError("not enough stock available checking for critical stock");
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
                        var (bloodBags, Count) = await _bloodBagRepository.GetAllAsync(1, request.Request.RequiredQty, filters);
                        foreach (var bloodBag in bloodBags)
                        {
                            bloodBag.UseBloodBag(request.Request.Id);
                            stock.UpdateCounts(stock.CountExpired, stock.CountExpiring, stock.ReadyCount - Count);
                            await _bloodBagRepository.UpdateAsync(bloodBag);
                            await _globalStockRepository.UpdateAsync(stock);
                            request.Request.UpdateAquiredQty();
                            request.Request.BloodSacs.Add(bloodBag);
                        }
                        request.Request.Resolve();
                        await _requestRepository.UpdateAsync(request.Request);
                        _logger.LogInformation("request resolved successfully");
                        _logger.LogInformation($"Assigned {bloodBags.Count} blood bags to critical priority request {request.Request.Id}");
                    }
                    else
                    {
                        _logger.LogInformation("not enough stock available for the request");
                        var topic = _kafkaSettings.Value.Topics["BloodRequests"];
                        if (request.Request.ServiceId == null)
                        {
                            _logger.LogError("ServiceId is null");
                            throw new NotFoundException("ServiceId is null", "AutoRequestResolverEvent");
                        }
                        var Service = await _serviceRepository.GetByIdAsync(request.Request.ServiceId.Value);
                        if (Service == null)
                        {
                            _logger.LogError("Service not found");
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
                        await _eventProducer.ProduceAsync(topic,JsonSerializer.Serialize(@event));           // publish kafka message to notify public donors   
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
                var (bloodBagsNormal, countNormal) = await _bloodBagRepository.GetAllAsync(1, request.Request.RequiredQty, filtersNormal);
                foreach (var bloodBag in bloodBagsNormal)
                {
                    bloodBag.UseBloodBag(request.Request.Id);
                    stock.UpdateCounts(stock.CountExpired, stock.CountExpiring, stock.ReadyCount - countNormal);
                    request.Request.BloodSacs.Add(bloodBag);
                    request.Request.UpdateAquiredQty();
                    await _bloodBagRepository.UpdateAsync(bloodBag);
                    await _globalStockRepository.UpdateAsync(stock);
                }
                request.Request.Resolve();
                await _requestRepository.UpdateAsync(request.Request);
                _logger.LogInformation("request resolved successfully");
                _logger.LogInformation($"Assigned {bloodBagsNormal.Count} blood bags to critical priority request {request.Request.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling AutoRequestResolverEvent");
            }
        }
    }    
}