using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class WilayaConfiguration : IEntityTypeConfiguration<Wilaya>
    {
        public void Configure(EntityTypeBuilder<Wilaya> builder)
        {
            builder.HasKey(w => w.Id);
            
            builder.Property(w => w.Name)
                   .IsRequired()
                   .HasMaxLength(100);
            
            // Define one-to-many relationship
            builder.HasMany(w => w.Communes)
                   .WithOne(c => c.Wilaya)
                   .HasForeignKey(c => c.WilayaId)
                   .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
        }
    }
}