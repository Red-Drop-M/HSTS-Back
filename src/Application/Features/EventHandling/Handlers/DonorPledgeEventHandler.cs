using MediatR;
using Polly;
using Domain.Events;
using Domain.Repositories;
using Application.Interfaces;
using Domain.ValueObjects;
using Domain.Entities;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Application.Features.EventHandling.Commands;
using Infrastructure.Configuration;
using Shared.Exceptions;

namespace Application.Features.EventHandling.Handlers
{
    public class DonorPledgeEventHandler
        : IRequestHandler<DonorPledgeCommand, Unit>
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IDonorRepository _donorRepository;
        private readonly IPledgeRepository _pledgeRepository;
        private readonly ILogger<DonorPledgeEventHandler> _logger;
        private readonly IEventProducer _eventProducer;
        private readonly RetryPolicySettings _retrySettings;

        public DonorPledgeEventHandler(
            IDonorRepository donorRepository,
            IRequestRepository requestRepository,
            IPledgeRepository pledgeRepository,
            ILogger<DonorPledgeEventHandler> logger,
            IEventProducer eventProducer,
            IOptions<RetryPolicySettings> retrySettings)
        {   
            _donorRepository = donorRepository;
            _requestRepository = requestRepository;
            _pledgeRepository = pledgeRepository;
            _logger = logger;
            _eventProducer = eventProducer;
            _retrySettings = retrySettings.Value;
        }

        public async Task<Unit> Handle(
            DonorPledgeCommand command, 
            CancellationToken ct)
        {
            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    _retrySettings.MaxRetryCount,
                    attempt => TimeSpan.FromMilliseconds(
                        _retrySettings.InitialDelayMs * 
                        Math.Pow(_retrySettings.BackoffExponent, attempt - 1)),
                    onRetry: (ex, delay) => 
                    {
                        _logger.LogWarning(ex, 
                            "Retrying pledge processing in {Delay}ms", 
                            delay.TotalMilliseconds);
                    });

            try
            {
                return await policy.ExecuteAsync(async () => 
                {
                    await ProcessPledge(command.Payload, ct);
                    return Unit.Value;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pledge processing failed after retries");
                await _eventProducer.ProduceAsync(
                    new PledgeFailedEvent(
                        command.Payload,
                        ex.Message,
                        DateTime.UtcNow,
                        Guid.NewGuid()),
                    "pledge-failed-events");
                throw;
            }
        }

       private async Task ProcessPledge(DonorPledgeEvent payload, CancellationToken ct)
{
    var request = await _requestRepository.GetByIdAsync(payload.RequestId)
        ?? throw new NotFoundException($"Request {payload.RequestId} not found","donor-pledge consumer");

    var donor = await _donorRepository.GetByNINAsync(payload.Donor.NIN);

    if (donor == null)
    {
        donor = new Donor(
            payload.Donor.DonorName,
            payload.Donor.Email,
            request.BloodType,
            payload.Donor.LastDonationDate,
            payload.Donor.Address,
            payload.Donor.NIN,
            payload.Donor.PhoneNumber,
            payload.Donor.DateOfBirth);
        await _donorRepository.AddAsync(donor);
    }

    var pledge = new DonorPledge(
        donor.Id,  // Use ID not name
        request.Id,
        payload.Status,
        payload.PledgedAt);

    await _pledgeRepository.AddAsync(pledge);
  // Commit transaction
}
    }
}