using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class CommuneConfiguration : IEntityTypeConfiguration<Commune>
    {
        public void Configure(EntityTypeBuilder<Commune> builder)
        {
            builder.HasKey(c => c.Id);
            
            
            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(100);
            
            // Foreign key property (already defined in relationship)
            builder.Property(c => c.WilayaId)
                   .IsRequired();
                   
            // Create index on foreign key for better query performance
            builder.HasIndex(c => c.WilayaId);
        }
    }
}