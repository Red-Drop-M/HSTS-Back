using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Events;
using Domain.Repositories;
using Domain.ValueObjects;
using Application.Features.EventHandling.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Microsoft.Extensions.Options;
using Infrastructure.Configuration;

namespace Application.Features.EventHandling.Handlers
{
    public class PledgeCanceledEventHandler : IRequestHandler<PledgeCanceledCommand>
    {
        private readonly IPledgeRepository _pledgeRepository;
        private readonly ILogger<PledgeCanceledEventHandler> _logger;
        private readonly RetryPolicySettings _retrySettings;
        
        public PledgeCanceledEventHandler(
            IPledgeRepository pledgeRepository,
            ILogger<PledgeCanceledEventHandler> logger,
            IOptions<RetryPolicySettings> retrySettings)
        {
            _pledgeRepository = pledgeRepository;
            _logger = logger;
            _retrySettings = retrySettings.Value;
        }
        
        public async Task<Unit> Handle(PledgeCanceledCommand command, CancellationToken cancellationToken)
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
                            "Retrying pledge cancellation processing in {Delay}ms", 
                            delay.TotalMilliseconds);
                    });

            try
            {
                return await policy.ExecuteAsync(async () => 
                {
                    await ProcessPledgeCancellation(command.Payload, cancellationToken);
                    return Unit.Value;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pledge cancellation processing failed after retries");
                throw;
            }
        }
        
        private async Task ProcessPledgeCancellation(PledgeCanceledEvent payload, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing pledge cancellation from external system for donor {DonorId} and request {RequestId}",
                payload.DonorId, 
                payload.RequestId);
            
            // Get the pledge
            var pledge = await _pledgeRepository.GetByDonorAndRequestIdAsync(
                payload.DonorId, 
                payload.RequestId);
            
            if (pledge == null)
            {
                _logger.LogWarning(
                    "Attempted to cancel non-existent pledge for donor {DonorId} and request {RequestId}",
                    payload.DonorId, 
                    payload.RequestId);
                return;
            }
            
            // Check if pledge can be canceled (not already fulfilled or canceled)
            if (pledge.Status.IsFulfilled || pledge.Status.IsCanceled)
            {
                _logger.LogWarning(
                    "Cannot cancel pledge with status {Status} for donor {DonorId} and request {RequestId}",
                    pledge.Status.Value,
                    payload.DonorId, 
                    payload.RequestId);
                return;
            }
            
            // Update status to canceled
            pledge.UpdateStatus(PledgeStatus.Canceled);
            await _pledgeRepository.UpdateAsync(pledge);
            
            _logger.LogInformation(
                "Pledge for donor {DonorId} and request {RequestId}}",
                payload.DonorId,
                payload.RequestId);
        }
    }
}