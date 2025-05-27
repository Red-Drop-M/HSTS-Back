using FastEndpoints;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Repositories;
using Domain.Events;
using Shared.Exceptions;
using Application.Interfaces;
using Infrastructure.ExternalServices.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Infrastructure.Services;

namespace Application.Features.EventHandling.Handlers
{
    public class AutoRequestResolverHandler : IEventHandler<AutoReuqestResolverEvent>
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IWebPushService _webPushService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IEventProducer _eventProducer;
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly ILogger<AutoRequestResolverHandler> _logger;
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        public AutoRequestResolverHandler(IRequestRepository requestRepository, IEventProducer eventProducer, 
            IOptions<KafkaSettings> kafkaSettings, ILogger<AutoRequestResolverHandler> logger, 
            IGlobalStockRepository globalStockRepository, IBloodBagRepository bloodBagRepository, 
            IBackgroundTaskQueue backgroundTaskQueue, IWebPushService webPushService, 
            INotificationRepository notificationRepository)
        {
            _bloodBagRepository = bloodBagRepository;
            _eventProducer = eventProducer;
            _kafkaSettings = kafkaSettings;
            _webPushService = webPushService;
            _notificationRepository = notificationRepository;
            _requestRepository = requestRepository;
            _logger = logger;
            _globalStockRepository = globalStockRepository;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public async Task HandleAsync(AutoReuqestResolverEvent request, CancellationToken ct)
        {
            // Queue the work to be done in a background thread with its own scope
            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (scope, token) =>
            {
                // Get new instances of dependencies from the scope
                var requestRepository = scope.ServiceProvider.GetRequiredService<IRequestRepository>();
                var globalStockRepository = scope.ServiceProvider.GetRequiredService<IGlobalStockRepository>();
                var bloodBagRepository = scope.ServiceProvider.GetRequiredService<IBloodBagRepository>();
                var eventProducer = scope.ServiceProvider.GetRequiredService<IEventProducer>();
                var kafkaSettings = scope.ServiceProvider.GetRequiredService<IOptions<KafkaSettings>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<AutoRequestResolverHandler>>();
                var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var webPushService = scope.ServiceProvider.GetRequiredService<IWebPushService>();

                // Process with fresh dependencies
                try
                {
                    var stock = await globalStockRepository.GetByKeyAsync(request.Request.BloodType, request.Request.BloodBagType);
                    if (stock == null)
                    {
                        logger.LogError("no stock found for the request");
                        return; // Just return, no need to throw exception
                    }

                    bool canAutoResolve = false;
                    int availableBagCount = 0;
                    List<string> compatibleTypes = new List<string>();

                    // Check if we have enough stock for normal resolution
                    if (stock.ReadyCount >= request.Request.RequiredQty)
                    {
                        canAutoResolve = true;
                        availableBagCount = stock.ReadyCount;
                        compatibleTypes.Add(request.Request.BloodType.Value);
                    }
                    // Check if we have enough stock for critical resolution
                    else if (stock.ReadyCount - request.Request.RequiredQty < stock.MinStock)
                    {
                        if ((stock.ReadyCount - request.Request.RequiredQty >= stock.CriticalStock) && 
                            request.Request.Priority.Value == Priority.Critical().Value)
                        {
                            // Define blood type compatibility mapping
                            Dictionary<string, List<string>> compatibilityMap = new Dictionary<string, List<string>>
                            {
                                // Recipient can receive from these blood types
                                { "O+", new List<string> { "O+", "O-" } },
                                { "O-", new List<string> { "O-" } },
                                { "A+", new List<string> { "A+", "A-", "O+", "O-" } },
                                { "A-", new List<string> { "A-", "O-" } },
                                { "B+", new List<string> { "B+", "B-", "O+", "O-" } },
                                { "B-", new List<string> { "B-", "O-" } },
                                { "AB+", new List<string> { "AB+", "AB-", "A+", "A-", "B+", "B-", "O+", "O-" } },
                                { "AB-", new List<string> { "AB-", "A-", "B-", "O-" } }
                            };

                            // Get compatible blood types for the requested blood type
                            compatibleTypes = compatibilityMap.ContainsKey(request.Request.BloodType.Value)
                                ? compatibilityMap[request.Request.BloodType.Value]
                                : new List<string> { request.Request.BloodType.Value };

                            // Check if we have enough compatible blood bags
                            var filters = new BloodBagFilter
                            {
                                BloodTypes = compatibleTypes,
                                BloodBagType = request.Request.BloodBagType,
                                Status = BloodBagStatus.Ready(),
                                RequestId = null
                            };

                            var (compatibleBags, count) = await bloodBagRepository.GetAllAsync(1, request.Request.RequiredQty, filters);
                            canAutoResolve = compatibleBags.Count >= request.Request.RequiredQty;
                            availableBagCount = compatibleBags.Count;
                        }
                    }

                    // If we can auto-resolve, send a notification to the user
                    if (canAutoResolve && request.Request.ServiceId.HasValue)
                    {
                        // Create notification in the database
                        var notification = new Notification(
                            title: "Blood Request Can Be Auto-Resolved",
                            message: $"Your request for {request.Request.RequiredQty} units of {request.Request.BloodType.Value} {request.Request.BloodBagType.Value} can be automatically fulfilled with {availableBagCount} available units.",
                            type: "AutoResolvable",
                            userId: request.Request.ServiceId,
                            link: $"/requests/auto-resolve/{request.Request.Id}",
                            icon: null
                        );

                        await notificationRepository.AddAsync(notification);
                        logger.LogInformation("Created notification for auto-resolvable request {RequestId}", request.Request.Id);

                        // Send web push notification
                        await webPushService.SendNotificationAsync(
                            notification.Title,
                            notification.Message,
                            notification.Type,
                            notification.UserId,
                            notification.Link
                        );

                        logger.LogInformation("Sent push notification for auto-resolvable request {RequestId}", request.Request.Id);
                        
                        // Update the request to mark it as auto-resolvable
                        request.Request.MarkAsAutoResolvable();
                        await requestRepository.UpdateAsync(request.Request);
                    }
                    else if (!canAutoResolve)
                    {
                        logger.LogInformation("Request {RequestId} cannot be auto-resolved. Not enough stock available.", request.Request.Id);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while checking auto-resolvability for request {RequestId}", request.Request.Id);
                }
            });
        }
    }    
}