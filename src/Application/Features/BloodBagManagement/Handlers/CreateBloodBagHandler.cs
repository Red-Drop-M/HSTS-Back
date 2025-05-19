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

        public CreateBloodBagHandler(IBloodBagRepository bloodBagRepository, ILogger<CreateBloodBagHandler> logger, IEventProducer eventProducer,
            IRequestRepository requestRepository, IDonorRepository donorRepository, IPledgeRepository pledgeRepository, IOptions<KafkaSettings> kafkaSettings)
        {
            _eventProducer = eventProducer;
            _requestRepository = requestRepository;
            _pledgeRepository = pledgeRepository;
            _bloodBagRepository = bloodBagRepository;
            _donorRepository = donorRepository;
            _kafkaSettings = kafkaSettings;
            _logger = logger;
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
                        var updateRequestEvent = new UpdateRequestEvent(
                            request.Id,
                            null,
                            request.AquiredQty,
                            request.RequiredQty,null);
                        await _eventProducer.ProduceAsync( JsonSerializer.Serialize(updateRequestEvent),topic);
                        _logger.LogInformation("Produced event: {Event}", updateRequestEvent);          
                    }
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
