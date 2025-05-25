using System;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IWebPushService
    {
        Task SaveSubscriptionAsync(PushSubscriptionDTO subscription, Guid? userId);
        Task SendNotificationAsync(string title, string message, string type, Guid? userId = null, string? link = null, string? icon = null);
        Task UnsubscribeAsync(string endpoint);
        Task<string> GetPublicKeyAsync();
    }
}