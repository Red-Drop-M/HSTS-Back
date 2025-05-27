using Application.DTOs;
using Application.Features.BloodRequests.Commands;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.BloodRequests.Handlers
{
    public class DeleteRequestHandler : IRequestHandler<DeleteRequestCommand, (RequestDto? request, BaseException? err)>
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IPledgeRepository _pledgeRepository;
        private readonly ILogger<DeleteRequestHandler> _logger;

        public DeleteRequestHandler(
            IRequestRepository requestRepository,
            IPledgeRepository pledgeRepository,
            ILogger<DeleteRequestHandler> logger)
        {
            _requestRepository = requestRepository;
            _pledgeRepository = pledgeRepository;
            _logger = logger;
        }

        public async Task<(RequestDto? request, BaseException? err)> Handle(
            DeleteRequestCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(command.Id);
                if (request == null)
                {
                    _logger.LogError("Request with ID {RequestId} not found", command.Id);
                    return (null, new NotFoundException($"Request {command.Id} not found", "delete request"));
                }

                // Cancel all related pledges
                var pledges = await _pledgeRepository.GetByRequestIdAsync(command.Id);
                if (pledges.Any())
                {
                    _logger.LogInformation("Canceling {Count} pledges related to deleted request {RequestId}", 
                        pledges.Count(), command.Id);
                    
                    foreach (var pledge in pledges)
                    {
                        // Mark the pledge as canceled
                        pledge.UpdateStatus(PledgeStatus.Canceled);
                        await _pledgeRepository.UpdateAsync(pledge);
                        
                        _logger.LogDebug("Canceled pledge from donor {DonorId} for request {RequestId}", 
                            pledge.DonorId, pledge.RequestId);
                    }
                }

                // Mark the request as deleted
                request.MarkAsDeleted();
                await _requestRepository.UpdateAsync(request);

                _logger.LogInformation("Request {RequestId} marked as deleted", request.Id);

                var requestDto = new RequestDto
                {
                    Id = request.Id,
                    Priority = request.Priority.Value,
                    BloodType = request.BloodType.Value,
                    BloodBagType = request.BloodBagType.Value,
                    RequestDate = request.RequestDate,
                    DueDate = request.DueDate,
                    Status = request.Status.Value,
                    MoreDetails = request.MoreDetails,
                    RequiredQty = request.RequiredQty,
                    AquiredQty = request.AquiredQty,
                    ServiceId = request.ServiceId,
                    DonorId = request.DonorId
                };

                return (requestDto, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Failed to delete request {RequestId}", command.Id);
                return (null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting request {RequestId}", command.Id);
                return (null, new InternalServerException("Failed to delete request", "delete request"));
            }
        }
    }
}