using Application.DTOs;
using Application.Features.BloodRequests.Commands;
using Domain.Entities;
using Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.BloodRequests.Handlers
{
    public class DeleteRequestHandler : IRequestHandler<DeleteRequestCommand, (RequestDto? request, BaseException? err)>
    {
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<DeleteRequestHandler> _logger;

        public DeleteRequestHandler(IRequestRepository requestRepository, ILogger<DeleteRequestHandler> logger)
        {
            _requestRepository = requestRepository;
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

                await _requestRepository.DeleteAsync(request.Id);

                var requestDto = new RequestDto
                {
                    Id = request.Id,
                    Priority = request.Priority,
                    BloodType = request.BloodType,
                    BloodBagType = request.BloodBagType,
                    RequestDate = request.RequestDate,
                    DueDate = request.DueDate,
                    Status = request.Status,
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