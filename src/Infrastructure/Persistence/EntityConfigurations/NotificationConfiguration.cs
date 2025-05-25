using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);
            
            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(n => n.Type)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(n => n.CreatedAt)
                .IsRequired();
                
            builder.Property(n => n.IsRead)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(n => n.Link)
                .HasMaxLength(255);
                
            builder.Property(n => n.Icon)
                .HasMaxLength(255);
                
            // Create indexes for faster queries
            builder.HasIndex(n => n.UserId);
            builder.HasIndex(n => n.IsRead);
            builder.HasIndex(n => n.CreatedAt);
        }
    }
}