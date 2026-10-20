using BCrypt.Net;
using Handicraft.Domain.Entities;

namespace Handicraft.Infrastructure.Persistence.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Ensure DB created (migrations should be applied by caller)

        // Seed roles and admin user if not exists
        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            var adminPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD");
            if (string.IsNullOrWhiteSpace(adminPassword))
                throw new InvalidOperationException("SEED_ADMIN_PASSWORD must be configured before seeding the initial admin user.");

            var admin = new User
            {
                Id = Guid.NewGuid(),
                Phone = "09120000000",
                Username = "09120000000",
                Name = "Admin",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword)
            };

            await context.Users.AddAsync(admin);
            await context.SaveChangesAsync();
        }
    }
}
