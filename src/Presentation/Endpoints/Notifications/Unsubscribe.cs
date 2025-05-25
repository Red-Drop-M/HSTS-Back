using FastEndpoints;
using Microsoft.Extensions.Logging;
using Application.Interfaces;
using Shared.Exceptions;
using System;

namespace Presentation.Endpoints.Notifications
{
    public class Unsubscribe : Endpoint<UnsubscribeRequest, UnsubscribeResponse>
    {
        private readonly IWebPushService _webPushService;
        private readonly ILogger<Unsubscribe> _logger;

        public Unsubscribe(IWebPushService webPushService, ILogger<Unsubscribe> logger)
        {
            _webPushService = webPushService;
            _logger = logger;
        }

        public override void Configure()
        {
            Post("/notifications/unsubscribe");
            Description(x => x
                .WithName("UnsubscribeFromPushNotifications")
                .WithTags("Notifications")
                .Produces<UnsubscribeResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(UnsubscribeRequest req, CancellationToken ct)
        {
            try
            {
                await _webPushService.UnsubscribeAsync(req.Endpoint);
                
                await SendAsync(new UnsubscribeResponse
                {
                    Success = true,
                    Message = "Successfully unsubscribed from push notifications"
                }, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from push notifications");
                throw new InternalServerException("Failed to unsubscribe from push notifications", "unsubscribe");
            }
        }
    }

    public class UnsubscribeRequest
    {
        public required string Endpoint { get; set; }
    }

    public class UnsubscribeResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}