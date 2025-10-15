using EstateHub.ListingService.DataAccess.SqlServer.Db;
using Microsoft.EntityFrameworkCore;

namespace EstateHub.ListingService.API;

public class Seed
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<Seed> _logger;

    public Seed(ApplicationDbContext context, ILogger<Seed> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedDataContextAsync()
    {
        try
        {
            _logger.LogInformation("Starting data initialization");

            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            var hasPendingMigrations = pendingMigrations.Any();

            if (hasPendingMigrations)
            {
                _logger.LogInformation($"Found {pendingMigrations.Count()} pending migrations. Applying...");
                await _context.Database.MigrateAsync();
                _logger.LogInformation("All migrations applied successfully");
            }
            else
            {
                _logger.LogInformation("No pending migrations found");
            }

            _logger.LogInformation("Data initialization completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during data initialization");
            throw;
        }
    }
}
