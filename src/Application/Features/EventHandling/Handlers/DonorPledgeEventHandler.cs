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
using Infrastructure.ExternalServices.Kafka;

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
        private readonly IOptions<KafkaSettings> _kafkaSettings;

        public DonorPledgeEventHandler(
            IDonorRepository donorRepository,
            IRequestRepository requestRepository,
            IPledgeRepository pledgeRepository,
            ILogger<DonorPledgeEventHandler> logger,
            IEventProducer eventProducer,
            IOptions<RetryPolicySettings> retrySettings,
            IOptions<KafkaSettings> kafkaSettings)
        {   
            _donorRepository = donorRepository;
            _requestRepository = requestRepository;
            _pledgeRepository = pledgeRepository;
            _logger = logger;
            _eventProducer = eventProducer;
            _retrySettings = retrySettings.Value;
            _kafkaSettings = kafkaSettings;
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
                var topic = _kafkaSettings.Value.Topics["PledgeFailed"];
                await _eventProducer.ProduceAsync(topic, JsonSerializer.Serialize(new PledgeFailedEvent(command.Payload, ex.Message, DateTime.UtcNow, Guid.NewGuid())));
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
        _logger.LogInformation("Creating new donor with NIN {NIN} and name {Name}", 
            payload.Donor.NIN, payload.Donor.DonorName ?? "(null)");

        // Add null check for DonorName
        string donorName = payload.Donor.DonorName ?? "Unknown Donor";

        donor = new Donor(
            donorName,  // Use name with null check
            payload.Donor.Email,
            request.BloodType,
            payload.Donor.LastDonationDate,
            payload.Donor.Address,
            payload.Donor.NIN,
            payload.Donor.PhoneNumber,
            payload.Donor.DateOfBirth);
            
        await _donorRepository.AddAsync(donor);
    }
    _logger.LogInformation("Donor found: {Donor}", donor.Id);
    // Check for existing pledge to prevent duplicate tracking exceptions
    var existingPledge = await _pledgeRepository.GetByDonorAndRequestIdAsync(donor.Id, request.Id);
    if (existingPledge != null)
    {
        _logger.LogInformation("Pledge already exists for donor {DonorId} and request {RequestId}, skipping creation", 
            donor.Id, request.Id);
        return;
    }

    // Convert date to UTC for PostgreSQL compatibility
    object pledgeDate = payload.PledgedAt;

    // Handle DateTime type
    if (pledgeDate is DateTime dateTime)
    {
        if (dateTime.Kind != DateTimeKind.Utc)
        {
            pledgeDate = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }

    // Convert to DateOnly properly
    DateOnly pledgeDateOnly;

    if (pledgeDate is DateTime dtValue)
    {
        pledgeDateOnly = DateOnly.FromDateTime(dtValue);
    }
    else if (pledgeDate is DateOnly doValue)
    {
        pledgeDateOnly = doValue;
    }
    else
    {
        // Fallback to current date
        pledgeDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);
        _logger.LogWarning("Could not determine pledge date, using current date");
    }

    // Create pledge with DateOnly
    var pledge = new DonorPledge(
        donor.Id,
        request.Id,
        payload.Status,
        pledgeDateOnly);
        
    _logger.LogInformation("Pledge created: {Pledge}", pledge);
    
    try
    {
        await _pledgeRepository.AddAsync(pledge);
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("already being tracked"))
    {
        // This can happen in concurrent scenarios or during retries
        _logger.LogWarning("Entity tracking conflict detected: {Message}", ex.Message);
        
        // Just log it and continue - the entity exists so our goal is accomplished
        _logger.LogInformation("Pledge was likely already created in a previous attempt");
    }
}
    }
}