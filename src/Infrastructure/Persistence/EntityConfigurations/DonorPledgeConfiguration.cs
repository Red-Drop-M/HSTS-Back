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
            builder.HasKey(dp => new { dp.DonorId, dp.RequestId });

            builder.Property(dp => dp.DonorId)
                .IsRequired();

            builder.Property(dp => dp.RequestId)
                .IsRequired();

            builder.Property(dp => dp.Status)
                .IsRequired()
                .HasConversion(
                    s => s.Value, 
                    val => PledgeStatus.FromString(val)
                );

            builder.Property(dp => dp.PledgeDate)
                .IsRequired();
        }
    }
}