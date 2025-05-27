using MediatR;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Application.Features.BloodRequests.Commands;
using Domain.Repositories;
using Domain.ValueObjects;
using Shared.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Features.BloodRequests.Handlers
{
    public class AutoResolveRequestHandler : IRequestHandler<AutoResolveRequestCommand, (RequestDto? request, BaseException? err)>
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<AutoResolveRequestHandler> _logger;
        private readonly IWebPushService _webPushService;

        public AutoResolveRequestHandler(
            IRequestRepository requestRepository,
            IBloodBagRepository bloodBagRepository,
            IGlobalStockRepository globalStockRepository,
            INotificationRepository notificationRepository,
            IWebPushService webPushService,
            ILogger<AutoResolveRequestHandler> logger)
        {
            _requestRepository = requestRepository;
            _bloodBagRepository = bloodBagRepository;
            _globalStockRepository = globalStockRepository;
            _notificationRepository = notificationRepository;
            _webPushService = webPushService;
            _logger = logger;
        }

        public async Task<(RequestDto? request, BaseException? err)> Handle(
            AutoResolveRequestCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                // Fetch the request
                var request = await _requestRepository.GetByIdAsync(command.Id);
                if (request == null)
                {
                    _logger.LogError("Request with ID {RequestId} not found", command.Id);
                    return (null, new NotFoundException($"Request {command.Id} not found", "auto-resolve request"));
                }

                // Check if the request is eligible for auto-resolution
                // The "autoResolve" field was added to the Request entity
                if (!request.autoResolve)
                {
                    _logger.LogError("Request {RequestId} is not marked as auto-resolvable", command.Id);
                    return (null, new BadRequestException("This request is not eligible for auto-resolution", "auto-resolve request"));
                }

                // Find available blood bags that match the criteria
                var compatibleTypes = new List<string> { request.BloodType.Value };
                
                // For critical requests, use the blood compatibility map
                if (request.Priority.Value == Priority.Critical().Value)
                {
                    Dictionary<string, List<string>> compatibilityMap = new Dictionary<string, List<string>>
                    {
                        { "O+", new List<string> { "O+", "O-" } },
                        { "O-", new List<string> { "O-" } },
                        { "A+", new List<string> { "A+", "A-", "O+", "O-" } },
                        { "A-", new List<string> { "A-", "O-" } },
                        { "B+", new List<string> { "B+", "B-", "O+", "O-" } },
                        { "B-", new List<string> { "B-", "O-" } },
                        { "AB+", new List<string> { "AB+", "AB-", "A+", "A-", "B+", "B-", "O+", "O-" } },
                        { "AB-", new List<string> { "AB-", "A-", "B-", "O-" } }
                    };

                    compatibleTypes = compatibilityMap.ContainsKey(request.BloodType.Value)
                        ? compatibilityMap[request.BloodType.Value]
                        : compatibleTypes;
                }

                var filters = new BloodBagFilter
                {
                    BloodTypes = compatibleTypes,
                    BloodBagType = request.BloodBagType,
                    Status = BloodBagStatus.Ready(),
                    RequestId = null
                };

                var (availableBags, _) = await _bloodBagRepository.GetAllAsync(1, request.RequiredQty, filters);
                if (availableBags.Count < request.RequiredQty)
                {
                    _logger.LogError("Not enough blood bags available to fulfill request {RequestId}", command.Id);
                    return (null, new BadRequestException("Not enough blood bags available to fulfill this request", "auto-resolve request"));
                }

                // Assign the blood bags to the request
                var selectedBags = availableBags.Take(request.RequiredQty).ToList();
                foreach (var bag in selectedBags)
                {
                    bag.AssignToRequest(request.Id);
                    await _bloodBagRepository.UpdateAsync(bag);
                }

                // Update global stock counts
                var stock = await _globalStockRepository.GetByKeyAsync(request.BloodType, request.BloodBagType);
                if (stock != null)
                {
                    stock.DecrementAvailableCount(request.RequiredQty);
                    await _globalStockRepository.UpdateAsync(stock);
                }

                // Create a notification about the resolution
                if (request.ServiceId.HasValue)
                {
                    var notification = new Notification(
                        title: "Blood Request Auto-Resolved",
                        message: $"Your request for {request.RequiredQty} units of {request.BloodType.Value} {request.BloodBagType.Value} has been automatically fulfilled.",
                        type: "RequestResolved",
                        userId: request.ServiceId,
                        link: $"/requests/{request.Id}"
                    );
                    
                    await _notificationRepository.AddAsync(notification);

                    // Send web push notification
                    await _webPushService.SendNotificationAsync(
                        notification.Title,
                        notification.Message,
                        notification.Type,
                        notification.UserId,
                        notification.Link
                    );

                    _logger.LogInformation("Sent push notification for auto-resolved request {RequestId}", request.Id);
                }

                // Mark the request as resolved
                request.Resolve();
                await _requestRepository.UpdateAsync(request);

                // Return the DTO
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
                _logger.LogError(ex, "Failed to auto-resolve request {RequestId}", command.Id);
                return (null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error auto-resolving request {RequestId}", command.Id);
                return (null, new InternalServerException("Failed to auto-resolve request", "auto-resolve request"));
            }
        }
    }
}