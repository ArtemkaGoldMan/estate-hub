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
            
            // Assign User role to existing users without roles
            await AssignUserRoleToUsersWithoutRolesAsync();
            
            // Seed admin user if it doesn't exist
            await SeedAdminUserAsync();
            
            // Seed test users for listing service seed data (3 users total)
            await SeedTestUserAsync(
                Guid.Parse("00000000-0000-0000-0000-000000000001"), 
                "anna.kowalska@email.com", 
                "Anna Kowalska",
                "+48 123 456 789",
                "Poland",
                "Warsaw"
            );
            await SeedTestUserAsync(
                Guid.Parse("00000000-0000-0000-0000-000000000002"), 
                "piotr.nowak@email.com", 
                "Piotr Nowak",
                "+48 234 567 890",
                "Poland",
                "Warsaw"
            );
            await SeedTestUserAsync(
                Guid.Parse("00000000-0000-0000-0000-000000000003"), 
                "maria.wisniewska@email.com", 
                "Maria Wiśniewska",
                "+48 345 678 901",
                "Poland",
                "Warsaw"
            );
            
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
    
    private async Task SeedTestUserAsync(Guid userId, string email, string displayName, string? phoneNumber = null, string? country = null, string? city = null)
    {
        const string testUserPassword = "Test12345!@#"; // Must be at least 12 characters
        
        // Check if test user already exists
        var testUser = await _userManager.FindByIdAsync(userId.ToString());
        if (testUser != null)
        {
            _logger.LogInformation("Test user already exists: {Email}", email);
            // Update phone number if provided and not set
            if (!string.IsNullOrEmpty(phoneNumber) && string.IsNullOrEmpty(testUser.PhoneNumber))
            {
                testUser.PhoneNumber = phoneNumber;
                testUser.Country = country;
                testUser.City = city;
                await _userManager.UpdateAsync(testUser);
                _logger.LogInformation("Updated test user contact info: {Email}", email);
            }
            return;
        }
        
        // Create test user with specific ID
        _logger.LogInformation("Creating test user for seed data: {Email}", email);
        testUser = new UserEntity
        {
            Id = userId,
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            DisplayName = displayName,
            PhoneNumber = phoneNumber,
            Country = country,
            City = city,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await _userManager.CreateAsync(testUser, testUserPassword);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to create test user {Email}: {Errors}", 
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }
        
        // Assign User role
        var userRole = await _roleManager.FindByNameAsync("User");
        if (userRole != null)
        {
            await _userManager.AddToRoleAsync(testUser, "User");
            _logger.LogInformation("Test user created successfully with email: {Email}, phone: {Phone}", email, phoneNumber ?? "N/A");
        }
        else
        {
            _logger.LogWarning("User role not found. Test user created but without User role.");
        }
    }

    private async Task AssignUserRoleToUsersWithoutRolesAsync()
    {
        try
        {
            var userRole = await _roleManager.FindByNameAsync("User");
            if (userRole == null)
            {
                _logger.LogWarning("User role not found. Cannot assign roles to existing users.");
                return;
            }

            // Get all users
            var allUsers = _userManager.Users.ToList();
            var usersWithoutRoles = new List<UserEntity>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles == null || roles.Count == 0)
                {
                    usersWithoutRoles.Add(user);
                }
            }

            if (usersWithoutRoles.Count > 0)
            {
                _logger.LogInformation("Found {Count} users without roles. Assigning User role...", usersWithoutRoles.Count);
                
                foreach (var user in usersWithoutRoles)
                {
                    // Skip admin user - it will be handled by SeedAdminUserAsync
                    if (user.Email == "admin@example.com")
                    {
                        continue;
                    }

                    var result = await _userManager.AddToRoleAsync(user, "User");
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Assigned User role to user: {Email}", user.Email);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to assign User role to user {Email}: {Errors}", 
                            user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            else
            {
                _logger.LogInformation("All users already have roles assigned.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning User role to existing users");
        }
    }
}
