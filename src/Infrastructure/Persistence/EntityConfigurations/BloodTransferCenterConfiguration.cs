using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class BloodTransferCenterConfiguration : IEntityTypeConfiguration<BloodTransferCenter>
    {
        public void Configure(EntityTypeBuilder<BloodTransferCenter> builder)
        {
            builder.HasKey(btc => btc.Id);
            
            builder.Property(btc => btc.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedNever();
            
            builder.Property(btc => btc.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(btc => btc.Address)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(btc => btc.Email)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(btc => btc.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(btc => btc.WilayaId)
                .IsRequired();
                
            // Add property for IsPrimary flag
            builder.Property(btc => btc.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false);
            
            // Create a unique constraint on IsPrimary when true
            // This ensures only one record can have IsPrimary = true
            builder.HasIndex(btc => btc.IsPrimary)
                .IsUnique()
                .HasFilter("\"IsPrimary\" = true");
                
            // Define relationship with Wilaya
            builder.HasOne(btc => btc.Wilaya)
                .WithMany(w => w.BloodTransferCenters)
                .HasForeignKey(btc => btc.WilayaId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Create index on WilayaId for performance
            builder.HasIndex(btc => btc.WilayaId);
            
            // Create unique index on name
            builder.HasIndex(btc => btc.Name).IsUnique();
            
            // Create index on email for quick lookups
            builder.HasIndex(btc => btc.Email);
        }
    }
}