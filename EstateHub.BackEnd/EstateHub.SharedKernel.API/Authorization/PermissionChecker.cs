using System.Security.Claims;

namespace EstateHub.SharedKernel.API.Authorization;

/// <summary>
/// Helper class for checking permissions in other microservices.
/// This eliminates the need to call AuthService for every permission check.
/// </summary>
public static class PermissionChecker
{
    /// <summary>
    /// Check if the current user has a specific permission
    /// </summary>
    public static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;
            
        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
            
        return PermissionDefinitions.HasPermission(permission, userRoles);
    }
    
    /// <summary>
    /// Check if the current user has any of the specified permissions
    /// </summary>
    public static bool HasAnyPermission(ClaimsPrincipal user, params string[] permissions)
    {
        return permissions.Any(permission => HasPermission(user, permission));
    }
    
    /// <summary>
    /// Check if the current user has all of the specified permissions
    /// </summary>
    public static bool HasAllPermissions(ClaimsPrincipal user, params string[] permissions)
    {
        return permissions.All(permission => HasPermission(user, permission));
    }
    
    /// <summary>
    /// Get all permissions for the current user
    /// </summary>
    public static List<string> GetUserPermissions(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return new List<string>();
            
        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
            
        var allPermissions = new List<string>();
        foreach (var role in userRoles)
        {
            allPermissions.AddRange(PermissionDefinitions.GetPermissionsForRole(role));
        }
        
        return allPermissions.Distinct().ToList();
    }
}
