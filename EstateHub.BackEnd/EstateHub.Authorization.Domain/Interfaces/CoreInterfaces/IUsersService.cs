using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;

namespace EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;

/// <summary>
/// Service interface for user management operations.
/// Provides methods for retrieving, updating, and managing user accounts, including admin operations.
/// </summary>
public interface IUsersService
{
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <typeparam name="TProjectTo">The type to project the user data to (e.g., GetUserResponse, GetUserWithRolesResponse).</typeparam>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users in the search.</param>
    /// <returns>A Result containing the projected user data if found, or an error if the user doesn't exist.</returns>
    Task<Result<TProjectTo>> GetByIdAsync<TProjectTo>(Guid id, bool includeDeleted)
        where TProjectTo : class;

    /// <summary>
    /// Retrieves multiple users by their unique identifiers (batch lookup).
    /// </summary>
    /// <typeparam name="TProjectTo">The type to project the user data to.</typeparam>
    /// <param name="ids">List of user unique identifiers to retrieve.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users in the search.</param>
    /// <returns>A Result containing a list of projected user data for the found users.</returns>
    Task<Result<List<TProjectTo>>> GetByIdsAsync<TProjectTo>(List<Guid> ids, bool includeDeleted)
        where TProjectTo : class;

    /// <summary>
    /// Updates user profile information.
    /// Users can only update their own profile information.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="request">The user update request containing the fields to update.</param>
    /// <returns>A Result indicating success or failure of the update operation.</returns>
    Task<Result> UpdateByIdAsync(Guid id, UserUpdateRequest request);

    /// <summary>
    /// Soft-deletes a user account.
    /// Users can only delete their own account.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>A Result indicating success or failure of the deletion operation.</returns>
    Task<Result> DeleteByIdAsync(Guid id);

    // Admin methods

    /// <summary>
    /// Retrieves a paginated list of all users. Admin only.
    /// </summary>
    /// <typeparam name="TProjectTo">The type to project the user data to.</typeparam>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users in the results.</param>
    /// <returns>A Result containing a paginated result with projected user data.</returns>
    Task<Result<PagedResult<TProjectTo>>> GetUsersAsync<TProjectTo>(int page, int pageSize, bool includeDeleted)
        where TProjectTo : class;

    /// <summary>
    /// Retrieves user statistics for the admin dashboard.
    /// </summary>
    /// <returns>A Result containing user statistics including total, active, suspended, and new users this month.</returns>
    Task<Result<UserStatsDto>> GetUserStatsAsync();

    /// <summary>
    /// Assigns a role to a user. Admin only.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to assign the role to.</param>
    /// <param name="role">The role name to assign (e.g., "Admin", "User").</param>
    /// <returns>A Result indicating success or failure of the role assignment.</returns>
    Task<Result> AssignUserRoleAsync(Guid userId, string role);

    /// <summary>
    /// Removes a role from a user. Admin only.
    /// Prevents users from removing their own Admin role.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to remove the role from.</param>
    /// <param name="role">The role name to remove.</param>
    /// <param name="currentUserId">The unique identifier of the current user performing the operation. Used to prevent self-removal of Admin role.</param>
    /// <returns>A Result indicating success or failure of the role removal.</returns>
    Task<Result> RemoveUserRoleAsync(Guid userId, string role, Guid? currentUserId = null);

    /// <summary>
    /// Suspends a user account with a reason. Admin only.
    /// Suspended users cannot log in until reactivated.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to suspend.</param>
    /// <param name="reason">The reason for suspending the user account.</param>
    /// <returns>A Result indicating success or failure of the suspension operation.</returns>
    Task<Result> SuspendUserAsync(Guid userId, string reason);

    /// <summary>
    /// Activates a previously suspended user account. Admin only.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to activate.</param>
    /// <returns>A Result indicating success or failure of the activation operation.</returns>
    Task<Result> ActivateUserAsync(Guid userId);

    /// <summary>
    /// Permanently deletes a user account. Admin only.
    /// This is a hard delete operation that cannot be undone.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <returns>A Result indicating success or failure of the deletion operation.</returns>
    Task<Result> AdminDeleteUserAsync(Guid userId);
}

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
/// <param name="Items">The list of items for the current page.</param>
/// <param name="Total">The total number of items across all pages.</param>
/// <param name="Page">The current page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
public record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize);

/// <summary>
/// Contains user statistics for the admin dashboard.
/// </summary>
/// <param name="TotalUsers">The total number of users in the system.</param>
/// <param name="ActiveUsers">The number of active (non-suspended) users.</param>
/// <param name="SuspendedUsers">The number of suspended users.</param>
/// <param name="NewUsersThisMonth">The number of users created in the current month.</param>
public record UserStatsDto(int TotalUsers, int ActiveUsers, int SuspendedUsers, int NewUsersThisMonth);
