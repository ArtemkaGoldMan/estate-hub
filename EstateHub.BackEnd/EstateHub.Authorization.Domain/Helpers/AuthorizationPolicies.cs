namespace EstateHub.Authorization.Domain.Helpers;

/// <summary>
/// Centralized authorization policies for the EstateHub system.
/// This makes it easy to manage permissions across all microservices.
/// </summary>
public static class AuthorizationPolicies
{
    // Core system policies
    public const string AdminAccess = "AdminAccess";
    public const string ModerationAccess = "ModerationAccess";
    public const string UserAccess = "UserAccess";
    
    // Future policies for other microservices
    public const string ListingManagement = "ListingManagement";
    public const string UserManagement = "UserManagement";
    public const string SystemSettings = "SystemSettings";
    public const string AnalyticsAccess = "AnalyticsAccess";
    
    /// <summary>
    /// Gets the roles required for a specific policy
    /// </summary>
    public static string[] GetRequiredRoles(string policyName)
    {
        return policyName switch
        {
            AdminAccess => new[] { "Admin" },
            ModerationAccess => new[] { "Admin", "Moderator" },
            UserAccess => new[] { "Admin", "Moderator", "User" },
            ListingManagement => new[] { "Admin", "Moderator", "User" },
            UserManagement => new[] { "Admin", "Moderator" },
            SystemSettings => new[] { "Admin" },
            AnalyticsAccess => new[] { "Admin", "Moderator" },
            _ => throw new ArgumentException($"Unknown policy: {policyName}")
        };
    }
    
    /// <summary>
    /// Checks if a user with given roles can access a specific policy
    /// </summary>
    public static bool CanAccess(string policyName, IEnumerable<string> userRoles)
    {
        var requiredRoles = GetRequiredRoles(policyName);
        return userRoles.Any(role => requiredRoles.Contains(role));
    }
}
