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
            // Primary Key
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd(); // Use this for PostgreSQL
                
            // Foreign Keys
            builder.HasOne(r => r.Donor)
                .WithMany(d => d.Requests) // Assuming Donor has a collection of Requests
                .HasForeignKey(r => r.DonorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(r => r.Service)
                .WithMany(s => s.Requests) // Assuming Service has a collection of Requests
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Property configurations
            builder.Property(r => r.Priority)
                .IsRequired()
                .HasConversion(
                    p => p.Value,  // Convert Priority object to string
                    p => Priority.Convert(p));

            builder.Property(r => r.BloodBagType)
                .IsRequired()
                .HasConversion(
                    b => b.Value,  // Convert BloodBagType object to string
                    b => BloodBagType.Convert(b));

            builder.Property(r => r.RequestDate)
                .IsRequired();

            builder.Property(r => r.Status)
                .IsRequired()
                .HasConversion(
                    s => s.Value,  // Convert RequestStatus object to string
                    s => RequestStatus.Convert(s));

            builder.Property(r => r.MoreDetails)
                .HasMaxLength(500);

            builder.Property(r => r.BloodType)
                .IsRequired()
                .HasConversion(
                    b => b.Value,  
                    b => BloodType.FromString(b));

            builder.Property(r => r.AquiredQty)
                .IsRequired(); // Ensuring AquiredQty is required

            builder.Property(r => r.DueDate)
                .IsRequired(false); // Ensuring DueDate can be null
        }
    }
}
