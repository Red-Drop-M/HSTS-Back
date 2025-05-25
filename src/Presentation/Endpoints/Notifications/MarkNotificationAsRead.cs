using FastEndpoints;
using Microsoft.Extensions.Logging;
using Application.Interfaces;
using Shared.Exceptions;
using System;

namespace Presentation.Endpoints.Notifications
{
    public class MarkNotificationAsRead : Endpoint<MarkNotificationAsReadRequest, MarkNotificationAsReadResponse>
    {
        private readonly ILogger<MarkNotificationAsRead> _logger;
        private readonly INotificationService _notificationService;

        public MarkNotificationAsRead(ILogger<MarkNotificationAsRead> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public override void Configure()
        {
            Put("/notifications/{id}/read");
            Roles("Admin", "User");
            Description(x => x
                .WithName("MarkNotificationAsRead")
                .WithTags("Notifications")
                .Produces<MarkNotificationAsReadResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(MarkNotificationAsReadRequest req, CancellationToken ct)
        {
            try
            {
                // Extract user ID from claims for authorization checking if needed
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    throw new UnauthorizedException("User not authenticated", "MarkNotificationAsRead");
                }

                await _notificationService.MarkAsReadAsync(req.Id);
                
                _logger.LogInformation("Marked notification {NotificationId} as read for user {UserId}", 
                    req.Id, userId);

                await SendAsync(new MarkNotificationAsReadResponse
                {
                    Success = true,
                    Message = "Notification marked as read"
                }, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                _logger.LogError(ex, "Error marking notification as read");
                throw new InternalServerException("An error occurred while processing your request", "MarkNotificationAsRead");
            }
        }
    }

    public class MarkNotificationAsReadRequest
    {
        public Guid Id { get; set; }
    }

    public class MarkNotificationAsReadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}