using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class DonorConfiguration : IEntityTypeConfiguration<Donor>
    {
        public void Configure(EntityTypeBuilder<Donor> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use this for PostgreSQL


            // Donor -> BloodBags (1:N)
            builder.HasMany(d => d.BloodBags)
                .WithOne(b => b.Donor)
                .HasForeignKey(b => b.DonorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Name
            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Email
            builder.Property(d => d.Email)
                .IsRequired()
                .HasMaxLength(100);

            // Phone Number
            builder.Property(d => d.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);

            // NIN (National Identification Number)
            builder.Property(d => d.NIN)
                .IsRequired()
                .HasMaxLength(20); // Adjust length as needed

            // Address
            builder.Property(d => d.Address)
                .IsRequired()
                .HasMaxLength(200);

            // Date of Birth
            builder.Property(d => d.DateOfBirth)
                .IsRequired();

            // Last Donation Date
            builder.Property(d => d.LastDonationDate)
                .IsRequired();

            // BloodType (ValueObject) with conversion
            builder.Property(d => d.BloodType)
                .IsRequired()
                .HasConversion(
                    bt => bt.Value,
                    val => BloodType.FromString(val)
                );
        }
    }
}
