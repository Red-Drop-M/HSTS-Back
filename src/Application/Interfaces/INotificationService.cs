using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string title, string message, string type, Guid? userId = null, string? link = null, string? icon = null);
        Task<List<NotificationDTO>> GetUserNotificationsAsync(Guid userId, bool includeRead = false);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
    }
}