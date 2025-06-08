using AutoMapper;
using EstateHub.Authorization.DataAccess.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace EstateHub.Authorization.API;

public class Seed
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<Seed> _logger;

    public Seed(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<Seed> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                await _context.Database.EnsureCreatedAsync();
            }

            // seed initial data
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing test data");
            throw;
        }
    }
}
