using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
    {
        public void Configure(EntityTypeBuilder<PushSubscription> builder)
        {
            builder.HasKey(s => s.Id);
            
            builder.Property(s => s.Endpoint)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(s => s.P256dh)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(s => s.Auth)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(s => s.CreatedAt)
                .IsRequired();
                
            // Create indexes for faster lookups
            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.Endpoint).IsUnique();
        }
    }
}