using MediatR;
using Domain.Repositories;
using Application.DTOs;
using Application.Features.PledgeManagement.Commands;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Domain.ValueObjects;

namespace Application.Features.PledgeManagement.Handlers
{
    public class UpdatePledgeStatusHandler : IRequestHandler<UpdatePledgeStatusCommand, (DonorPledgeDTO pledge, BaseException? error)>
    {
        private readonly IPledgeRepository _pledgeRepository;
        private readonly ILogger<UpdatePledgeStatusHandler> _logger;

        public UpdatePledgeStatusHandler(IPledgeRepository pledgeRepository, ILogger<UpdatePledgeStatusHandler> logger)
        {
            _pledgeRepository = pledgeRepository;
            _logger = logger;
        }

        public async Task<(DonorPledgeDTO pledge, BaseException? error)> Handle(UpdatePledgeStatusCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var pledge = await _pledgeRepository.GetByDonorAndRequestIdAsync(command.DonorId, command.RequestId);
                
                if (pledge == null)
                {
                    _logger.LogWarning("Pledge not found for donor {DonorId} and request {RequestId}", 
                        command.DonorId, command.RequestId);
                    return (new DonorPledgeDTO(), new NotFoundException($"Pledge not found for donor {command.DonorId} and request {command.RequestId}", "update-pledge-status"));
                }
                
                // Validate state transitions
                if (!IsValidStatusTransition(pledge.Status, command.Status))
                {
                    _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for pledge", 
                        pledge.Status.Value, command.Status.Value);
                    return (new DonorPledgeDTO(), new BadRequestException($"Invalid status transition from {pledge.Status.Value} to {command.Status.Value}", "update-pledge-status"));
                }
                
                // Update status
                pledge.UpdateStatus(command.Status);
                
                // Save changes
                await _pledgeRepository.UpdateAsync(pledge);
                
                _logger.LogInformation("Pledge status updated from {OldStatus} to {NewStatus} for donor {DonorId} and request {RequestId}",
                    pledge.Status.Value, command.Status.Value, command.DonorId, command.RequestId);
                
                // Return updated pledge
                return (new DonorPledgeDTO
                {
                    DonorId = pledge.DonorId,
                    RequestId = pledge.RequestId,
                    PledgedAt = pledge.PledgeDate,
                    Status = pledge.Status
                }, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error updating pledge status for donor {DonorId} and request {RequestId}", 
                    command.DonorId, command.RequestId);
                return (new DonorPledgeDTO(), ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating pledge status for donor {DonorId} and request {RequestId}", 
                    command.DonorId, command.RequestId);
                return (new DonorPledgeDTO(), new InternalServerException("Failed to update pledge status", "update-pledge-status"));
            }
        }
        
        private bool IsValidStatusTransition(PledgeStatus currentStatus, PledgeStatus newStatus)
        {
            // Implement status transition rules
            if (currentStatus.IsCanceled || currentStatus.IsFulfilled)
            {
                // Cannot update from terminal states
                return false;
            }
            
            // Basic validation - you can add more specific rules based on your business logic
            return true;
        }
    }
}