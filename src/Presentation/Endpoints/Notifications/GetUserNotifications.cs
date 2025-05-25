using FastEndpoints;
using Microsoft.Extensions.Logging;
using Application.Interfaces;
using Shared.Exceptions;
using System;

namespace Presentation.Endpoints.Notifications
{
    public class GetUserNotifications : Endpoint<GetUserNotificationsRequest, GetUserNotificationsResponse>
    {
        private readonly ILogger<GetUserNotifications> _logger;
        private readonly INotificationService _notificationService;

        public GetUserNotifications(ILogger<GetUserNotifications> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public override void Configure()
        {
            Get("/notifications");
            Roles("Admin", "User");
            Description(x => x
                .WithName("GetUserNotifications")
                .WithTags("Notifications")
                .Produces<GetUserNotificationsResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(GetUserNotificationsRequest req, CancellationToken ct)
        {
            try
            {
                // Extract user ID from claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    throw new UnauthorizedException("User not authenticated", "GetUserNotifications");
                }

                var notifications = await _notificationService.GetUserNotificationsAsync(
                    userId, 
                    req.IncludeRead);
                
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

                _logger.LogInformation("Retrieved {Count} notifications for user {UserId}", 
                    notifications.Count, userId);

                await SendAsync(new GetUserNotificationsResponse
                {
                    Notifications = notifications,
                    UnreadCount = unreadCount,
                    Success = true
                }, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                throw new InternalServerException("An error occurred while processing your request", "GetUserNotifications");
            }
        }
    }

    public class GetUserNotificationsRequest
    {
        public bool IncludeRead { get; set; } = false;
    }

    public class GetUserNotificationsResponse
    {
        public List<Application.DTOs.NotificationDTO> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
        public bool Success { get; set; }
    }
}