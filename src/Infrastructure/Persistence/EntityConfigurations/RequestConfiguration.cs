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
            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.Donor)
                .WithMany()
                .HasForeignKey(r => r.DonorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(r => r.Service)
                .WithMany()
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.SetNull);

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
        }
    }
}