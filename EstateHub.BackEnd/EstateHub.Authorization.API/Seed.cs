using AutoMapper;
using EstateHub.Authorization.DataAccess.SqlServer;
using EstateHub.Authorization.DataAccess.SqlServer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EstateHub.Authorization.API;

public class Seed
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<Seed> _logger;
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<RoleEntity> _roleManager;

    public Seed(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<Seed> logger,
        UserManager<UserEntity> userManager,
        RoleManager<RoleEntity> roleManager)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
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

            // Seed roles if they don't exist
            await SeedRolesAsync();
            
            // Seed admin user if it doesn't exist
            await SeedAdminUserAsync();
            
            // seed initial data
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing test data");
            throw;
        }
    }
    
    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "User" };
        
        foreach (var roleName in roles)
        {
            if (!await _context.Roles.AnyAsync(r => r.Name == roleName))
            {
                _logger.LogInformation($"Creating role: {roleName}");
                await _context.Roles.AddAsync(new EstateHub.Authorization.DataAccess.SqlServer.Entities.RoleEntity
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                });
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Roles seeded successfully");
    }
    
    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@example.com";
        const string adminPassword = "Admin12345";
        
        // Check if admin user already exists
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser != null)
        {
            _logger.LogInformation("Admin user already exists");
            
            // Ensure admin user has Admin role
            if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                _logger.LogInformation("Adding Admin role to existing admin user");
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
            return;
        }
        
        // Create admin user
        _logger.LogInformation("Creating admin user");
        adminUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true, // Skip email confirmation for admin
            NormalizedEmail = adminEmail.ToUpperInvariant(),
            NormalizedUserName = adminEmail.ToUpperInvariant(),
            DisplayName = "Administrator",
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await _userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to create admin user: {Errors}", 
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }
        
        // Assign Admin role
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        if (adminRole != null)
        {
            await _userManager.AddToRoleAsync(adminUser, "Admin");
            _logger.LogInformation("Admin user created successfully with email: {Email}", adminEmail);
        }
        else
        {
            _logger.LogWarning("Admin role not found. Admin user created but without Admin role.");
        }
    }
}
