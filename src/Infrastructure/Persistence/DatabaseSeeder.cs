using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                logger.LogInformation("Running database migrations...");
                await dbContext.Database.MigrateAsync();

                // Fix: Load all users first and then check in memory
                var anyAdminExists = await dbContext.Users
                    .AsNoTracking()  // For better performance since we're just reading
                    .ToListAsync()    // Get all users (this moves processing to client side)
                    .ContinueWith(t => t.Result.Any(u => u.Role.Role == UserRole.Admin().Role));

                if (!anyAdminExists)
                {
                    logger.LogInformation("Seeding admin user...");
                    var adminUser = new User(
                        name: "Admin",
                        email: "admin@hsts.com",
                        password: "Admin123!", // In a real app, this would be hashed
                        role: UserRole.Admin(),
                        dateOfBirth: new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Fix: Specify UTC DateTime
                        phoneNumber: "0000000000",
                        address: "Admin Address"
                    );

                    dbContext.Users.Add(adminUser);
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("Admin user seeded successfully");
                }

                // Additional seeding can be added here
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database seeding");
                throw;
            }
        }
    }
}