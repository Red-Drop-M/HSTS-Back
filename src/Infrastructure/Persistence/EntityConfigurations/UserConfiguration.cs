using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            // Primary Key
            builder.HasKey(u => u.Id);

            // Configure Id
            builder.Property(u => u.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("Id");

            // Configure Name
            builder.Property(u => u.Name)
                .HasMaxLength(100)
                .IsRequired();
            builder.HasIndex(u => u.Name).IsUnique();

            // Configure Email
            builder.Property(u => u.Email)
                .IsRequired();
            builder.HasIndex(u => u.Email).IsUnique();

            // Configure Password
            builder.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(50);

            // Configure Role (ValueObject) with conversion
            builder.Property(u => u.Role)
                .HasConversion(
                    r => r.Role, // Convert UserRole to string for storage
                    r => UserRole.Convert(r) // Convert string from DB back to UserRole
                )
                .IsRequired();

            // Configure DateOfBirth
            builder.Property(u => u.DateOfBirth)
                .IsRequired();

            // Configure PhoneNumber
            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(10)
                .IsRequired();
        }
    }
}
