using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IWebPushService _webPushService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            IWebPushService webPushService,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _webPushService = webPushService;
            _logger = logger;
        }

        public async Task CreateNotificationAsync(string title, string message, string type, Guid? userId = null, string? link = null, string? icon = null)
        {
            try
            {
                // Send web push notification (this also saves the notification to DB)
                await _webPushService.SendNotificationAsync(title, message, type, userId, link, icon);
                _logger.LogInformation("Created notification for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<NotificationDTO>> GetUserNotificationsAsync(Guid userId, bool includeRead = false)
        {
            try
            {
                var notifications = await _notificationRepository.GetByUserIdAsync(userId, includeRead);
                
                return notifications.Select(n => new NotificationDTO
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    UserId = n.UserId,
                    Link = n.Link
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                return await _notificationRepository.GetUnreadCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                throw;
            }
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                await _notificationRepository.MarkAsReadAsync(notificationId);
                _logger.LogInformation("Marked notification {NotificationId} as read", notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                throw;
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            try
            {
                await _notificationRepository.MarkAllAsReadAsync(userId);
                _logger.LogInformation("Marked all notifications as read for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                throw;
            }
        }
    }
}