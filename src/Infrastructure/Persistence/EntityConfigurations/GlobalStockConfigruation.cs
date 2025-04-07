using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class GlobalStockConfiguration : IEntityTypeConfiguration<GlobalStock>
    {
        public void Configure(EntityTypeBuilder<GlobalStock> builder)
        {
            // Composite key
            builder.HasKey(gs => new { gs.BloodType, gs.BloodBagType });

            // BloodType conversion (stored as string)
            builder.Property(gs => gs.BloodType)
                .IsRequired()
                .HasConversion(
                    bt => bt.Value,
                    value => BloodType.Convert(value));

            // BloodBagType conversion (stored as string)
            builder.Property(gs => gs.BloodBagType)
                .IsRequired()
                .HasConversion(
                    bbt => bbt.Value,
                    value => BloodBagType.Convert(value));

            // Scalar stock-related properties
            builder.Property(gs => gs.CountExpired)
                .IsRequired();

            builder.Property(gs => gs.CountExpiring)
                .IsRequired();

            builder.Property(gs => gs.ReadyCount)
                .IsRequired();

            builder.Property(gs => gs.MinStock)
                .IsRequired();

            builder.Property(gs => gs.CriticalStock)
                .IsRequired();
        }
    }
}
