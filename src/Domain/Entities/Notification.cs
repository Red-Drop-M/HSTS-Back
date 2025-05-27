using System;

namespace Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public string Type { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public bool IsRead { get; private set; }
        public Guid? UserId { get; private set; }
        public string? Link { get; private set; }
        public string? Icon { get; private set; }
        
        private Notification() { } // For EF Core
        
        public Notification(
            string title,
            string message,
            string type,
            Guid? userId = null,
            string? link = null,
            string? icon = null)
        {
            Id = Guid.NewGuid();
            Title = title;
            Message = message;
            Type = type;
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
            UserId = userId;
            Link = link;
            Icon = icon;
        }
        
        // Add this constructor for initialization with all properties including Id
        public Notification(
            Guid id,
            string title,
            string message,
            string type,
            DateTime createdAt,
            bool isRead,
            Guid? userId = null,
            string? link = null,
            string? icon = null)
        {
            Id = id;
            Title = title;
            Message = message;
            Type = type;
            CreatedAt = createdAt;
            IsRead = isRead;
            UserId = userId;
            Link = link;
            Icon = icon;
        }
        
        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}