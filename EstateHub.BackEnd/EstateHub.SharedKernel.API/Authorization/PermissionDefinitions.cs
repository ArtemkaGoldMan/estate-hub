namespace EstateHub.SharedKernel.API.Authorization;

/// <summary>
/// Centralized permission definitions for all EstateHub microservices.
/// This allows other microservices to check permissions without calling AuthService.
/// </summary>
public static class PermissionDefinitions
{
    // System permissions
    public const string SystemAdmin = "SystemAdmin";
    public const string UserManagement = "UserManagement";
    public const string ContentModeration = "ContentModeration";
    public const string RoleManagement = "RoleManagement";
    
    // Business permissions
    public const string CreateListings = "CreateListings";
    public const string ManageListings = "ManageListings";
    public const string ViewAnalytics = "ViewAnalytics";
    
    // Report permissions
    public const string CreateReports = "CreateReports";
    public const string ManageReports = "ManageReports";
    public const string ViewReports = "ViewReports";
    
    /// <summary>
    /// Maps roles to their permissions
    /// </summary>
    public static readonly Dictionary<string, List<string>> RolePermissions = new()
    {
        ["Admin"] = new List<string>
        {
            SystemAdmin, UserManagement, ContentModeration, RoleManagement,
            CreateListings, ManageListings, ViewAnalytics,
            CreateReports, ManageReports, ViewReports
        },
        
        ["User"] = new List<string>
        {
            CreateListings, ManageListings,
            CreateReports
        }
    };
    
    /// <summary>
    /// Check if a user with given roles has a specific permission
    /// </summary>
    public static bool HasPermission(string permission, IEnumerable<string> userRoles)
    {
        return userRoles.Any(role => 
            RolePermissions.ContainsKey(role) && 
            RolePermissions[role].Contains(permission));
    }
    
    /// <summary>
    /// Get all permissions for a specific role
    /// </summary>
    public static List<string> GetPermissionsForRole(string role)
    {
        return RolePermissions.ContainsKey(role) 
            ? RolePermissions[role] 
            : new List<string>();
    }
}
