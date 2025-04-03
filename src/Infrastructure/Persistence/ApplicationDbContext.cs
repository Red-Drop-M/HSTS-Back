using Microsoft.EntityFrameworkCore;
using Domain.Entities; // Ensure this namespace matches the location of your domain entities

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor accepting options injected via DI
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for each entity. These correspond to tables in the database.
        public DbSet<User> Users { get; set; }
        public DbSet<Donner> Orders { get; set; }
        public DbSet<BloodSac> OrderItems { get; set; }
        public DbSet<Request> Products { get; set; }

        // Override OnModelCreating to configure your entity mappings using the Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optionally, automatically apply all configurations from the assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Example of configuring a unique index on the Email property of the User entity:
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
