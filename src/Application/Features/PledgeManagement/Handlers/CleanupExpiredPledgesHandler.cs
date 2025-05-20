using Domain.Entities;
using Domain.ValueObjects;
using Application.Features.PledgeManagement.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PledgeManagement.Handlers
{
    public class CleanupExpiredPledgesHandler : IRequestHandler<CleanupExpiredPledgesCommand>
    {
        private readonly IPledgeRepository _pledgeRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<CleanupExpiredPledgesHandler> _logger;

        public CleanupExpiredPledgesHandler(
            IPledgeRepository pledgeRepository,
            IRequestRepository requestRepository,
            ILogger<CleanupExpiredPledgesHandler> logger)
        {
            _pledgeRepository = pledgeRepository;
            _requestRepository = requestRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(CleanupExpiredPledgesCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting expired pledges cleanup process");
            
            // Get all pledges with Pledged status
            var (pledges, _) = await _pledgeRepository.GetAllAsync(
                1,
                int.MaxValue,
                PledgeStatus.Pledged.Value);
                
            _logger.LogInformation("Retrieved {Count} active pledges for expiration check", pledges.Count);
            
            if (!pledges.Any())
            {
                _logger.LogInformation("No active pledges found to check");
                return Unit.Value;
            }

            // Current date for comparison
            var today = DateOnly.FromDateTime(DateTime.Now);
            _logger.LogInformation("Checking expiration against current date: {Today}", today);
            
            // Keep track of expired pledges
            var expiredPledges = new List<DonorPledge>();
            
            // Process each pledge
            foreach (var pledge in pledges)
            {
                // Skip pledges that are already canceled or fulfilled
                if (pledge.Status.IsCanceled || pledge.Status.IsFulfilled)
                {
                    continue;
                }

                // Get the associated request to check its due date
                var request = await _requestRepository.GetByIdAsync(pledge.RequestId);
                
                if (request == null)
                {
                    _logger.LogWarning("Request {RequestId} associated with pledge for donor {DonorId} not found", 
                        pledge.RequestId, pledge.DonorId);
                    continue;
                }
                
                // Check if the request's due date has passed
                if (request.DueDate < today)
                {
                    _logger.LogInformation("Pledge for donor {DonorId} on request {RequestId} is expired. Request due date: {DueDate}, Current date: {Today}", 
                        pledge.DonorId, pledge.RequestId, request.DueDate, today);
                    
                    // Update the status to canceled (since it's expired)
                    pledge.UpdateStatus(PledgeStatus.Canceled);
                    expiredPledges.Add(pledge);
                    
                    _logger.LogInformation("Pledge for donor {DonorId} on request {RequestId} marked as canceled due to expiration", 
                        pledge.DonorId, pledge.RequestId);
                }
            }
            
            // Update all expired pledges
            foreach (var pledge in expiredPledges)
            {
                await _pledgeRepository.UpdateAsync(pledge);
            }
            
            _logger.LogInformation("Pledge expiration check completed. Updated {Count} pledges", expiredPledges.Count);
            
            return Unit.Value;
        }
    }
}