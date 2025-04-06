using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Persistence.Configurations
{
    public class RequestConfiguration : IEntityTypeConfiguration<Request>
    {
        public void Configure(EntityTypeBuilder<Request> builder)
        {
            builder.HasKey(r => r.Id); // Primary Key

            // Configure foreign key relationship between Request and Donor
            builder.HasOne(r => r.Donor)
                .WithMany() // Donor can have many requests
                .HasForeignKey(r => r.DonnorId)
                .OnDelete(DeleteBehavior.SetNull); // When Donor is deleted, set foreign key to null

            // Configure foreign key relationship between Request and Service
            builder.HasOne(r => r.Service)
                .WithMany() // Service can have many requests
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.SetNull); // When Service is deleted, set foreign key to null

            // Configure the Priority property with a default value and map it to the Value property
            builder.Property(r => r.Priority)
                .HasConversion(p => p.Value, p => Priority.Convert(p)) // Convert between string and Priority
                .HasDefaultValue(Priority.Standard().Value); // Default value for Priority

            // Configure the BloodBagType property with a default value and map it to the Value property
            builder.Property(r => r.BloodBagType)
                .HasConversion(b => b.Value, b => BloodBagType.Convert(b)) // Convert between string and BloodBagType
                .HasDefaultValue(BloodBagType.Blood().Value); // Default value for BloodBagType

            // Configure the RequestDate property with a default value
            builder.Property(r => r.RequestDate)
                .HasDefaultValue(DateOnly.FromDateTime(DateTime.Now));

            // Configure the Status property with a default value
            builder.Property(r => r.Status)
                .HasDefaultValue(RequestStatus.Pending());

            // Configure other properties, if needed
            builder.Property(r => r.MoreDetails)
                .HasMaxLength(500); // Example: set max length for MoreDetails if needed
        }
    }
}
