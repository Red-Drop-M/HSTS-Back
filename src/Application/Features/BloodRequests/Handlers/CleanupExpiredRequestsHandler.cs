using Domain.Entities;
using Domain.ValueObjects;
using Application.Features.BloodRequests.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Interfaces;
using Domain.Events;
using Infrastructure.ExternalServices.Kafka;
using Domain.Repositories;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Application.Features.BloodRequests.Handlers
{ 
    public class CleanupExpiredBloodRequestsHandler : IRequestHandler<CleanupExpiredRequestsCommand>
    {
        private readonly IRequestRepository _bloodRequestRepository;
        private readonly IPledgeRepository _pledgeRepository;
        private readonly IEventProducer _eventProducer;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly ILogger<CleanupExpiredBloodRequestsHandler> _logger;

        public CleanupExpiredBloodRequestsHandler(
            IRequestRepository bloodRequestRepository,
            IPledgeRepository pledgeRepository,
            ILogger<CleanupExpiredBloodRequestsHandler> logger, IOptions<KafkaSettings> kafkaSettings, IEventProducer eventProducer)
        {
            _bloodRequestRepository = bloodRequestRepository;
            _pledgeRepository = pledgeRepository;
            _logger = logger;
            _kafkaSettings = kafkaSettings;
            _eventProducer = eventProducer;
        }

        public async Task<Unit> Handle(CleanupExpiredRequestsCommand command, CancellationToken cancellationToken)
        {
            var bloodRequests = await _bloodRequestRepository.GetAllAsync();
            var now = DateOnly.FromDateTime(DateTime.Now);
            var expiredRequests = bloodRequests
                .Where(br => br.DueDate < now && br.Status.Value != RequestStatus.Resolved().Value)
                .ToList();

            int expiredPledgesCount = 0;

            foreach (var request in expiredRequests)
            {
                // Mark the request as rejected/expired
                request.Reject();
                _logger.LogInformation("Blood request {RequestId} marked as expired. Expiry date: {ExpiryDate}", 
                    request.Id, request.DueDate);
                
                // Get all pledges for this request that are still in "Pledged" status
                var (pledges, _) = await _pledgeRepository.GetAllAsync(
                    1, 
                    int.MaxValue, 
                    PledgeStatus.Pledged.Value,
                    requestId: request.Id);
                
                // Update status for all pledges related to this expired request
                foreach (var pledge in pledges)
                {
                    // Skip pledges that are already in a terminal state
                    if (pledge.Status.IsCanceled || pledge.Status.IsFulfilled)
                    {
                        continue;
                    }
                    
                    // Update to canceled status (since the request expired)
                    pledge.UpdateStatus(PledgeStatus.Canceled);
                    await _pledgeRepository.UpdateAsync(pledge);
                    var topic = _kafkaSettings.Value.Topics["PledgeCanceled"];
                    var @event = new PledgeCanceledEvent(pledge.DonorId, pledge.RequestId);
                    await _eventProducer.ProduceAsync(topic, JsonSerializer.Serialize(@event));
                    expiredPledgesCount++;
                    
                    _logger.LogInformation(
                        "Pledge from donor {DonorId} for request {RequestId} marked as canceled due to request expiration", 
                        pledge.DonorId, pledge.RequestId);
                }
            }

            if (expiredRequests.Any())
            {
                // Check for requests that are close to expiration but partially fulfilled
                var closeToExpiryRequests = bloodRequests
                    .Where(br => br.DueDate >= now && 
                                br.DueDate <= now.AddDays(3))
                    .ToList();
                
                foreach (var request in closeToExpiryRequests)
                {
                    // This is close to expiry but partially fulfilled
                    _logger.LogWarning("Blood request {RequestId} is close to expiration with partial fulfillment. Due date: {DueDate}", 
                        request.Id, request.DueDate);
                }
                
                await _bloodRequestRepository.UpdateRangeAsync(expiredRequests);
                _logger.LogInformation("Updated {Count} expired blood requests and {PledgeCount} related pledges", 
                    expiredRequests.Count, expiredPledgesCount);
            }

            return Unit.Value;
        }
    }
}