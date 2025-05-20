using Domain.Entities;
using Domain.ValueObjects;
using Application.Features.BloodRequests.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Repositories;
namespace Application.Features.BloodRequests.Handlers
{ 
    public class CleanupExpiredBloodRequestsHandler : IRequestHandler<CleanupExpiredRequestsCommand>
    {
        private readonly IRequestRepository _bloodRequestRepository;
        private readonly ILogger<CleanupExpiredBloodRequestsHandler> _logger;

        public CleanupExpiredBloodRequestsHandler(IRequestRepository bloodRequestRepository, ILogger<CleanupExpiredBloodRequestsHandler> logger)
        {
            _bloodRequestRepository = bloodRequestRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(CleanupExpiredRequestsCommand command, CancellationToken cancellationToken)
        {
            var bloodRequests = await _bloodRequestRepository.GetAllAsync();
            var now = DateOnly.FromDateTime(DateTime.Now);
            var expiredRequests = bloodRequests
                .Where(br => br.DueDate < now && br.Status.Value != RequestStatus.Resolved().Value)
                .ToList();

            foreach (var request in expiredRequests)
            {
                request.Reject();
                _logger.LogInformation("Blood request {RequestId} marked as expired. Expiry date: {ExpiryDate}", 
                    request.Id, request.DueDate);
            }

            if (expiredRequests.Any())
            {
                // Check for requests that are close to expiration but partially fulfilled
                var closeToExpiryRequests = bloodRequests
                    .Where(br => br.DueDate >= now && 
                                br.DueDate <= now.AddDays(3) && 
                                br.Status.Value == RequestStatus.Partial().Value)
                    .ToList();
                
                foreach (var request in closeToExpiryRequests)
                {
                    // This is close to expiry but partially fulfilled
                    _logger.LogWarning("Blood request {RequestId} is close to expiration with partial fulfillment. Due date: {DueDate}", 
                        request.Id, request.DueDate);
                }
                await _bloodRequestRepository.UpdateRangeAsync(expiredRequests);
                _logger.LogInformation("Updated {Count} expired blood requests", expiredRequests.Count);
            }

            return Unit.Value;
        }
    }
}