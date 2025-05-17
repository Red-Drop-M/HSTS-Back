using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{ 
    public class DonorPledgeConfiguration : IEntityTypeConfiguration<DonorPledge>
    {
        public void Configure(EntityTypeBuilder<DonorPledge> builder)
        {
        // Composite primary key
            builder.HasKey(dp => new { dp.DonorName, dp.RequestId });
        
        // Relationship to Donor using Name as foreign key
            builder.HasOne(dp => dp.Donor)
                .WithMany(d => d.Pledges)
                .HasForeignKey(dp => dp.DonorName)
                .HasPrincipalKey(d => d.Name)  // Reference Donor's Name
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(dp => dp.Request)
                .WithMany(r => r.Pledges)
                .HasForeignKey(dp => dp.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

                // Property configurations
            builder.Property(dp => dp.PledgeDate)
                .HasConversion(
                    d => d.ToDateTime(TimeOnly.MinValue),
                    d => DateOnly.FromDateTime(d))
                .IsRequired();

           builder.Property(dp => dp.Status)
            .HasConversion(
                v => v.Value,  // Convert to string for storage
                v => PledgeStatus.FromString(v))  // Convert from string
            .IsRequired()
            .HasMaxLength(20); 
        }
    }
}