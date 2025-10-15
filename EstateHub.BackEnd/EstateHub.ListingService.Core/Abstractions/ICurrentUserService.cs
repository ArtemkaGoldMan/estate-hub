namespace EstateHub.ListingService.Core.Abstractions;

public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current authenticated user ID
    /// </summary>
    /// <returns>User ID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
    Guid GetUserId();
    
    /// <summary>
    /// Checks if the current user is in the specified role
    /// </summary>
    /// <param name="role">Role name to check</param>
    /// <returns>True if user is in the role, false otherwise</returns>
    bool IsInRole(string role);
}
