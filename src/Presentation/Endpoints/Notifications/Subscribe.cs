using FastEndpoints;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Application.Interfaces;
using Shared.Exceptions;
using System;

namespace Presentation.Endpoints.Notifications
{
    public class Subscribe : Endpoint<SubscribeRequest, SubscribeResponse>
    {
        private readonly IWebPushService _webPushService;
        private readonly ILogger<Subscribe> _logger;

        public Subscribe(IWebPushService webPushService, ILogger<Subscribe> logger)
        {
            _webPushService = webPushService;
            _logger = logger;
        }

        public override void Configure()
        {
            Post("/notifications/subscribe");
            Description(x => x
                .WithName("SubscribeToPushNotifications")
                .WithTags("Notifications")
                .Produces<SubscribeResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(SubscribeRequest req, CancellationToken ct)
        {
            try
            {
                // Get user ID from claims if authenticated
                Guid? userId = null;
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }

                var subscription = new PushSubscriptionDTO
                {
                    Endpoint = req.Endpoint,
                    Keys = new PushSubscriptionDTO.KeysInfo
                    {
                        P256dh = req.Keys.P256dh,
                        Auth = req.Keys.Auth
                    }
                };

                await _webPushService.SaveSubscriptionAsync(subscription, userId);
                
                await SendAsync(new SubscribeResponse
                {
                    Success = true,
                    Message = "Successfully subscribed to push notifications"
                }, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to push notifications");
                throw new InternalServerException("Failed to subscribe to push notifications", "subscribe");
            }
        }
    }

    public class SubscribeRequest
    {
        public required string Endpoint { get; set; }
        public required SubscriptionKeys Keys { get; set; }
        
        public class SubscriptionKeys
        {
            public required string P256dh { get; set; }
            public required string Auth { get; set; }
        }
    }

    public class SubscribeResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}