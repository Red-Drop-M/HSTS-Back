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
            // No dependencies on KafkaSettings here
        }

        // Define DbSets for each entity. These correspond to tables in the database.
        public DbSet<User> Users { get; set; }
        public DbSet<Donor> Donors { get; set; } // Renamed to reflect entity name
        public DbSet<BloodBag> BloodBags { get; set; } // Renamed to reflect entity name
        public DbSet<GlobalStock> GlobalStocks { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<DonorPledge> Pledges {get ; set ;} // Renamed to reflect entity name

        // Override OnModelCreating to configure your entity mappings using the Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
