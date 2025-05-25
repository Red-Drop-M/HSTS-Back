using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebPush;

namespace Infrastructure.Services
{
    public class WebPushService : IWebPushService
    {
        private readonly IPushSubscriptionRepository _subscriptionRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<WebPushService> _logger;
        private readonly string _publicKey;
        private readonly string _privateKey;
        private readonly string _subject;

        public WebPushService(
            IPushSubscriptionRepository subscriptionRepository,
            INotificationRepository notificationRepository,
            IConfiguration configuration,
            ILogger<WebPushService> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _notificationRepository = notificationRepository;
            _logger = logger;
            
            _publicKey = configuration["WebPush:PublicKey"];
            _privateKey = configuration["WebPush:PrivateKey"];
            _subject = configuration["WebPush:Subject"];
            
            if (string.IsNullOrEmpty(_publicKey) || string.IsNullOrEmpty(_privateKey) || string.IsNullOrEmpty(_subject))
            {
                throw new ArgumentException("WebPush configuration is missing or incomplete");
            }
        }

        public Task<string> GetPublicKeyAsync()
        {
            return Task.FromResult(_publicKey);
        }

        public async Task SaveSubscriptionAsync(PushSubscriptionDTO subscription, Guid? userId)
        {
            try
            {
                var existingSubscription = await _subscriptionRepository.GetByEndpointAsync(subscription.Endpoint);
                
                if (existingSubscription != null)
                {
                    // Update existing subscription
                    existingSubscription.Update(
                        subscription.Endpoint,
                        subscription.Keys.P256dh,
                        subscription.Keys.Auth);
                    
                    await _subscriptionRepository.UpdateAsync(existingSubscription);
                    _logger.LogInformation("Updated push subscription for user {UserId}", userId);
                }
                else
                {
                    // Create new subscription
                    var newSubscription = new Domain.Entities.PushSubscription(
                        userId,
                        subscription.Endpoint,
                        subscription.Keys.P256dh,
                        subscription.Keys.Auth);
                    
                    await _subscriptionRepository.AddAsync(newSubscription);
                    _logger.LogInformation("Created new push subscription for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving push subscription for user {UserId}", userId);
                throw;
            }
        }

        public async Task SendNotificationAsync(string title, string message, string type, Guid? userId = null, string? link = null, string? icon = null)
        {
            try
            {
                // Save notification to database
                var notification = new Notification(title, message, type, userId, link, icon);
                await _notificationRepository.AddAsync(notification);
                _logger.LogInformation("Created notification {NotificationId} for user {UserId}", notification.Id, userId);
                
                // Determine which subscriptions to send to
                List<Domain.Entities.PushSubscription> subscriptions;
                if (userId.HasValue)
                {
                    // Send to specific user's subscriptions
                    subscriptions = await _subscriptionRepository.GetByUserIdAsync(userId.Value);
                }
                else
                {
                    // Send to all subscriptions
                    subscriptions = await _subscriptionRepository.GetAllAsync();
                }
                
                if (subscriptions.Count == 0)
                {
                    _logger.LogInformation("No push subscriptions found for notification {NotificationId}", notification.Id);
                    return;
                }
                
                // Create payload
                var payload = JsonSerializer.Serialize(new
                {
                    notification.Id,
                    notification.Title,
                    notification.Message,
                    notification.Type,
                    notification.Link,
                    notification.Icon,
                    notification.CreatedAt
                });
                
                // Configure WebPush client
                var webPushClient = new WebPushClient();
                var vapidDetails = new VapidDetails(_subject, _publicKey, _privateKey);
                
                // Send push notifications
                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        // Fix: Use the correct fully qualified name for WebPush's PushSubscription
                        var pushSubscription = new WebPush.PushSubscription(
                            subscription.Endpoint,
                            subscription.P256dh,
                            subscription.Auth);
                        
                        await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
                        _logger.LogInformation(
                            "Sent push notification {NotificationId} to subscription {SubscriptionId}", 
                            notification.Id, 
                            subscription.Id);
                    }
                    catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone)
                    {
                        // Subscription has expired or been unsubscribed
                        _logger.LogWarning("Subscription {SubscriptionId} is gone, deleting", subscription.Id);
                        await _subscriptionRepository.DeleteAsync(subscription.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex, 
                            "Error sending push notification {NotificationId} to subscription {SubscriptionId}", 
                            notification.Id, 
                            subscription.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification");
                throw;
            }
        }

        public async Task UnsubscribeAsync(string endpoint)
        {
            try
            {
                await _subscriptionRepository.DeleteByEndpointAsync(endpoint);
                _logger.LogInformation("Unsubscribed push notification endpoint: {Endpoint}", endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing endpoint: {Endpoint}", endpoint);
                throw;
            }
        }
    }
}