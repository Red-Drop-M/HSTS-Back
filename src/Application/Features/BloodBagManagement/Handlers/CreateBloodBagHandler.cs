using Domain.Entities;
using Domain.Repositories;
using MediatR;
using Application.DTOs;
using Application.Features.BloodBagManagement.Commands;
using Shared.Exceptions;
using Domain.ValueObjects;
using Domain.Events;
using Application.Interfaces;
using Infrastructure.ExternalServices.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
namespace Application.Features.BloodBagManagement.Handlers
{
    public class CreateBloodBagHandler : IRequestHandler<CreateBloodBagCommand, (BloodBagDTO? bloodBag, BaseException? err)> 
    {
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly ILogger<CreateBloodBagHandler> _logger;
        private readonly IEventProducer _eventProducer;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly IDonorRepository _donorRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly IPledgeRepository _pledgeRepository;
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly IBloodTransferCenterRepository _centerRepository;

        public CreateBloodBagHandler(IBloodBagRepository bloodBagRepository, ILogger<CreateBloodBagHandler> logger, IEventProducer eventProducer,
            IRequestRepository requestRepository, IDonorRepository donorRepository, IPledgeRepository pledgeRepository, IOptions<KafkaSettings> kafkaSettings, IGlobalStockRepository globalStockRepository, IBloodTransferCenterRepository centerRepository)
        {
            _eventProducer = eventProducer;
            _requestRepository = requestRepository;
            _pledgeRepository = pledgeRepository;
            _bloodBagRepository = bloodBagRepository;
            _centerRepository = centerRepository;
            _donorRepository = donorRepository;
            _kafkaSettings = kafkaSettings;
            _logger = logger;
            _globalStockRepository = globalStockRepository;
        }

        public async Task<(BloodBagDTO? bloodBag , BaseException? err)> Handle(CreateBloodBagCommand bloodBag, CancellationToken cancellationToken)
        {
            try
            {
                var newBloodBag = new BloodBag(
                    bloodBag.BloodBagType,
                    bloodBag.BloodType,
                    bloodBag.Status,
                    bloodBag.ExpirationDate,
                    bloodBag.AcquiredDate,
                    bloodBag.DonorId,
                    bloodBag.RequestId);

                await _bloodBagRepository.AddAsync(newBloodBag);
                if (newBloodBag.RequestId != null)
                {
                    var request = await _requestRepository.GetByIdAsync(newBloodBag.RequestId.Value);
                    if (request == null)
                    {
                        throw new NotFoundException("Request not found", "fetching request updating pledge status");
                    }
                    var pledge = await _pledgeRepository.GetByDonorAndRequestIdAsync(newBloodBag.DonorId.Value, newBloodBag.RequestId.Value);
                    if (pledge != null)
                    {
                        pledge.UpdateStatus(PledgeStatus.Fulfilled);
                        await _pledgeRepository.UpdateAsync(pledge);
                        request.UpdateAquiredQty();
                        await _requestRepository.UpdateAsync(request);
                        var topic = _kafkaSettings.Value.Topics["UpdateRequest"];
                        if (request.RequiredQty == 0)
                        {
                            var message = new UpdateRequestEvent(
                                request.Id,
                                null,
                                RequestStatus.Resolved().Value,
                                request.AquiredQty,
                                request.RequiredQty, null);
                            await _eventProducer.ProduceAsync(topic,JsonSerializer.Serialize(message));
                        }
                        else
                        {
                            var updateRequestEvent = new UpdateRequestEvent(
                                request.Id,
                                null,
                                null,
                                request.AquiredQty,
                                request.RequiredQty, null);
                            await _eventProducer.ProduceAsync(topic,JsonSerializer.Serialize(updateRequestEvent));

                        }       
                    }
                }

                // Set expiration date based on blood bag type if not provided
                if (newBloodBag.ExpirationDate == null)
                {
                    // Use the value comparison instead of direct reference comparison
                    if (newBloodBag.BloodBagType.Value == BloodBagType.Blood().Value)
                    {
                        newBloodBag.UpdateExpirationDate(DateTime.Now.AddDays(30)); // Blood expires after 30 days
                    }
                    else if (newBloodBag.BloodBagType.Value == BloodBagType.Plaquette().Value)
                    {
                        newBloodBag.UpdateExpirationDate(DateTime.Now.AddDays(5)); // Plaquette expires after 5 days
                    }
                    else if (newBloodBag.BloodBagType.Value == BloodBagType.Plasma().Value)
                    {
                        newBloodBag.UpdateExpirationDate(DateTime.Now.AddDays(5)); // Plasma expires after 5 days
                    }
                    else
                    {
                        newBloodBag.UpdateExpirationDate(DateTime.Now.AddDays(30)); // Default to 30 days
                    }
                    await _bloodBagRepository.UpdateAsync(newBloodBag);
                    _logger.LogInformation("Blood bag expiration date updated to {ExpirationDate}", newBloodBag.ExpirationDate);
                    var stock = await _globalStockRepository.GetByKeyAsync(newBloodBag.BloodType, newBloodBag.BloodBagType);
                    if (stock == null) {
                        throw new NotFoundException("Global stock not found", "updating global stock");
                    }
                    stock?.IncrementAvailableCount(1);
                    await _globalStockRepository.UpdateAsync(stock);
                    var topic = _kafkaSettings.Value.Topics["GlobalStock"];
                    var subMessage = new GlobalStockData(
                        newBloodBag.BloodType,
                        newBloodBag.BloodBagType,
                        stock?.ReadyCount + stock?.CountExpiring + stock?.CountExpired ?? 0,
                        stock?.ReadyCount ?? 0,
                        stock?.MinStock ?? 0,
                        stock?.CountExpired ?? 0);
                    var hospital = await _centerRepository.GetPrimaryAsync();
                    var message = new GlobalStockEvent(
                        hospital.Id, 
                        subMessage
                    );
                    await _eventProducer.ProduceAsync(topic, JsonSerializer.Serialize(message));
                }
                return (new BloodBagDTO
                    {
                        Id = newBloodBag.Id,
                        BloodBagType = newBloodBag.BloodBagType,
                        BloodType = newBloodBag.BloodType,
                        Status = newBloodBag.Status,
                        ExpirationDate = newBloodBag.ExpirationDate,
                        AcquiredDate = newBloodBag.AcquiredDate,
                        DonorId = newBloodBag.DonorId ?? throw new InvalidOperationException("DonorId cannot be null"),
                        RequestId = newBloodBag.RequestId
                    }, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error creating blood bag");
                return (null, ex);
            }
        }
    }
}
