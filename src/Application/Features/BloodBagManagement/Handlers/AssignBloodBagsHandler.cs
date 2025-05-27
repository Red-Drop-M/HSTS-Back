using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.BloodBagManagement.Commands;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class AssignBloodBagsHandler : IRequestHandler<AssignBloodBagsCommand, (int assignedCount, BaseException? err)>
    {
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IWebPushService _webPushService;
        private readonly ILogger<AssignBloodBagsHandler> _logger;

        public AssignBloodBagsHandler(
            IBloodBagRepository bloodBagRepository,
            IRequestRepository requestRepository,
            IGlobalStockRepository globalStockRepository,
            INotificationRepository notificationRepository,
            IWebPushService webPushService,
            ILogger<AssignBloodBagsHandler> logger)
        {
            _bloodBagRepository = bloodBagRepository;
            _requestRepository = requestRepository;
            _globalStockRepository = globalStockRepository;
            _notificationRepository = notificationRepository;
            _webPushService = webPushService;
            _logger = logger;
        }

        public async Task<(int assignedCount, BaseException? err)> Handle(
            AssignBloodBagsCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Get the request
                var request = await _requestRepository.GetByIdAsync(command.RequestId);
                if (request == null)
                {
                    _logger.LogError("Request with ID {RequestId} not found", command.RequestId);
                    return (0, new NotFoundException($"Request {command.RequestId} not found", "assign blood bags"));
                }

                // Check if request is already resolved
                if (request.Status.Value == RequestStatus.Resolved().Value)
                {
                    _logger.LogError("Request {RequestId} is already resolved", command.RequestId);
                    return (0, new BadRequestException("This request is already resolved", "assign blood bags"));
                }

                // Validate all blood bag IDs exist
                var bloodBags = new List<BloodBag>();
                foreach (var id in command.BloodBagIds)
                {
                    var bloodBag = await _bloodBagRepository.GetByIdAsync(id);
                    if (bloodBag == null)
                    {
                        _logger.LogError("Blood bag with ID {BloodBagId} not found", id);
                        return (0, new NotFoundException($"Blood bag {id} not found", "assign blood bags"));
                    }

                    // Check if blood bag is available
                    if (bloodBag.RequestId != null)
                    {
                        _logger.LogError("Blood bag {BloodBagId} is already assigned to request {RequestId}", 
                            id, bloodBag.RequestId);
                        return (0, new BadRequestException($"Blood bag {id} is already assigned to another request", "assign blood bags"));
                    }

                    if (bloodBag.Status.Value != BloodBagStatus.Ready().Value)
                    {
                        _logger.LogError("Blood bag {BloodBagId} is not in Ready status", id);
                        return (0, new BadRequestException($"Blood bag {id} is not available for assignment", "assign blood bags"));
                    }

                    bloodBags.Add(bloodBag);
                }

                // Check blood type compatibility
                foreach (var bloodBag in bloodBags)
                {
                    bool isCompatible = IsBloodTypeCompatible(request.BloodType.Value, bloodBag.BloodType.Value);
                    if (!isCompatible)
                    {
                        _logger.LogError("Blood bag {BloodBagId} with blood type {BloodBagType} is not compatible with request blood type {RequestBloodType}", 
                            bloodBag.Id, bloodBag.BloodType.Value, request.BloodType.Value);
                        return (0, new BadRequestException($"Blood bag {bloodBag.Id} with blood type {bloodBag.BloodType.Value} is not compatible with request blood type {request.BloodType.Value}", "assign blood bags"));
                    }

                    // Check blood bag type matches request
                    if (bloodBag.BloodBagType.Value != request.BloodBagType.Value)
                    {
                        _logger.LogError("Blood bag {BloodBagId} with type {BloodBagType} does not match request blood bag type {RequestBloodBagType}", 
                            bloodBag.Id, bloodBag.BloodBagType.Value, request.BloodBagType.Value);
                        return (0, new BadRequestException($"Blood bag {bloodBag.Id} with type {bloodBag.BloodBagType.Value} does not match request blood bag type {request.BloodBagType.Value}", "assign blood bags"));
                    }
                }

                // Check if we're assigning more than needed
                int currentlyAssigned = request.AquiredQty;
                int toBeAssigned = bloodBags.Count;
                int totalAfterAssignment = currentlyAssigned + toBeAssigned;
                int required = request.RequiredQty;

                if (totalAfterAssignment > required)
                {
                    _logger.LogWarning("Attempting to assign more blood bags than required. Required: {Required}, Current: {Current}, To be assigned: {ToBeAssigned}", 
                        required, currentlyAssigned, toBeAssigned);
                    return (0, new BadRequestException($"Cannot assign {toBeAssigned} more blood bags. Request requires {required} units and already has {currentlyAssigned} units assigned.", "assign blood bags"));
                }

                // Assign blood bags to the request
                foreach (var bloodBag in bloodBags)
                {
                    bloodBag.UseBloodBag(request.Id);
                    await _bloodBagRepository.UpdateAsync(bloodBag);
                    _logger.LogInformation("Assigned blood bag {BloodBagId} to request {RequestId}", bloodBag.Id, request.Id);
                }

                // Update the request's acquired quantity
                request.UpdateAcquiredQuantity(totalAfterAssignment);

                // If all required blood bags are now assigned, mark the request as resolved
                if (totalAfterAssignment == required)
                {
                    request.Resolve();
                    _logger.LogInformation("Request {RequestId} marked as resolved after assigning blood bags", request.Id);
                }
                else
                {
                    // Otherwise mark as partial if not already
                    if (request.Status.Value == RequestStatus.Pending().Value)
                    {
                        request.MarkAsPartial();
                        _logger.LogInformation("Request {RequestId} marked as partial after assigning blood bags", request.Id);
                    }
                }

                await _requestRepository.UpdateAsync(request);

                // Update global stock counts
                var stockGroups = bloodBags
                    .GroupBy(b => new { BloodType = b.BloodType.Value, BloodBagType = b.BloodBagType.Value })
                    .ToList();

                foreach (var group in stockGroups)
                {
                    var bloodType = BloodType.FromString(group.Key.BloodType);
                    var bloodBagType = BloodBagType.Convert(group.Key.BloodBagType);
                    var count = group.Count();

                    var stock = await _globalStockRepository.GetByKeyAsync(bloodType, bloodBagType);
                    if (stock != null)
                    {
                        stock.DecrementAvailableCount(count);
                        await _globalStockRepository.UpdateAsync(stock);
                    }
                }

                // Create notification for the service about the assignment
                if (request.ServiceId.HasValue)
                {
                    string status = totalAfterAssignment == required ? "fulfilled" : "partially fulfilled";
                    
                    var notification = new Notification(
                        title: "Blood Request Update",
                        message: $"Your request for {request.RequiredQty} units of {request.BloodType.Value} {request.BloodBagType.Value} has been {status} with {toBeAssigned} new units.",
                        type: totalAfterAssignment == required ? "RequestResolved" : "RequestPartial",
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
                }

                return (bloodBags.Count, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Failed to assign blood bags to request {RequestId}", command.RequestId);
                return (0, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while assigning blood bags to request {RequestId}", command.RequestId);
                return (0, new InternalServerException("Failed to assign blood bags to request", "assign blood bags"));
            }
        }

        private bool IsBloodTypeCompatible(string requestBloodType, string donorBloodType)
        {
            // Blood type compatibility map (recipient -> compatible donors)
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

            if (compatibilityMap.TryGetValue(requestBloodType, out var compatibleTypes))
            {
                return compatibleTypes.Contains(donorBloodType);
            }

            // If blood type is not in the map, default to exact match only
            return requestBloodType == donorBloodType;
        }
    }
}