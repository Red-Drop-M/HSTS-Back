using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class BloodBagConfiguration : IEntityTypeConfiguration<BloodBag>
    {
        public void Configure(EntityTypeBuilder<BloodBag> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use this for PostgreSQL


            builder.HasOne(b => b.Donor)
                .WithMany(d => d.BloodBags)
                .HasForeignKey(b => b.DonorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(b => b.Request)
                .WithMany(r => r.BloodSacs)
                .HasForeignKey(b => b.RequestId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(b => b.BloodType)
                .IsRequired()
                .HasConversion(bt => bt.Value, val => BloodType.FromString(val));

            builder.Property(b => b.BloodBagType)
                .IsRequired()
                .HasConversion(bbt => bbt.Value, val => BloodBagType.Convert(val));

            builder.Property(b => b.Status)
                .IsRequired()
                .HasConversion(s => s.Value, val => BloodBagStatus.Convert(val));

            builder.Property(b => b.AcquiredDate)
                .IsRequired();

            builder.Property(b => b.ExpirationDate)
                .IsRequired();
        }
    }
}