using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.Models;

namespace EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;

/// <summary>
/// Repository interface for user data access operations.
/// Provides methods for querying, updating, and managing user entities in the data store.
/// </summary>
public interface IUsersRepository
{
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <typeparam name="TProjectTo">The type to project the user data to (e.g., UserDto, UserWithRolesDto).</typeparam>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users in the search. Defaults to false.</param>
    /// <returns>The projected user data if found, or null if the user doesn't exist.</returns>
    Task<TProjectTo?> GetByIdAsync<TProjectTo>(Guid id, bool includeDeleted = false)
        where TProjectTo : class;

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <typeparam name="TProjectTo">The type to project the user data to.</typeparam>
    /// <param name="email">The email address of the user.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users in the search. Defaults to false.</param>
    /// <returns>The projected user data if found, or null if the user doesn't exist.</returns>
    Task<TProjectTo?> GetByEmailAsync<TProjectTo>(string email, bool includeDeleted = false)
        where TProjectTo : class;

    /// <summary>
    /// Retrieves multiple users by their unique identifiers (batch lookup).
    /// </summary>
    /// <typeparam name="TProjectTo">The type to project the user data to.</typeparam>
    /// <param name="ids">List of user unique identifiers to retrieve.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users in the search. Defaults to false.</param>
    /// <returns>A list of projected user data for the found users.</returns>
    Task<List<TProjectTo>> GetByIdsAsync<TProjectTo>(List<Guid> ids, bool includeDeleted = false)
        where TProjectTo : class;

    /// <summary>
    /// Updates user information in the data store.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="user">The user update DTO containing the fields to update.</param>
    /// <returns>True if the update was successful, false otherwise.</returns>
    Task<bool> UpdateByIdAsync(Guid id, UserUpdateDto user);

    /// <summary>
    /// Soft-deletes a user by marking them as deleted in the data store.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>True if the deletion was successful, false otherwise.</returns>
    Task<bool> DeleteByIdAsync(Guid id);

    // Admin methods

    /// <summary>
    /// Retrieves a paginated list of all users. Admin only.
    /// </summary>
    /// <typeparam name="TProjectTo">The type to project the user data to.</typeparam>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users in the results. Defaults to false.</param>
    /// <returns>A list of projected user data for the requested page.</returns>
    Task<List<TProjectTo>> GetUsersAsync<TProjectTo>(int page, int pageSize, bool includeDeleted = false)
        where TProjectTo : class;

    /// <summary>
    /// Gets the total count of users in the system.
    /// </summary>
    /// <param name="includeDeleted">Whether to include soft-deleted users in the count. Defaults to false.</param>
    /// <returns>The total number of users.</returns>
    Task<int> GetUsersCountAsync(bool includeDeleted = false);

    /// <summary>
    /// Gets the count of active (non-suspended, non-deleted) users.
    /// </summary>
    /// <returns>The number of active users.</returns>
    Task<int> GetActiveUsersCountAsync();

    /// <summary>
    /// Gets the count of suspended users.
    /// </summary>
    /// <returns>The number of suspended users.</returns>
    Task<int> GetSuspendedUsersCountAsync();

    /// <summary>
    /// Gets the count of users created in the current month.
    /// </summary>
    /// <returns>The number of new users created this month.</returns>
    Task<int> GetNewUsersThisMonthCountAsync();

    /// <summary>
    /// Assigns a role to a user in the data store.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="role">The role name to assign (e.g., "Admin", "User").</param>
    /// <returns>True if the role assignment was successful, false otherwise.</returns>
    Task<bool> AssignUserRoleAsync(Guid userId, string role);

    /// <summary>
    /// Removes a role from a user in the data store.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="role">The role name to remove.</param>
    /// <returns>True if the role removal was successful, false otherwise.</returns>
    Task<bool> RemoveUserRoleAsync(Guid userId, string role);

    /// <summary>
    /// Suspends a user account by updating their status in the data store.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to suspend.</param>
    /// <param name="reason">The reason for suspending the user account.</param>
    /// <returns>True if the suspension was successful, false otherwise.</returns>
    Task<bool> SuspendUserAsync(Guid userId, string reason);

    /// <summary>
    /// Activates a previously suspended user account.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to activate.</param>
    /// <returns>True if the activation was successful, false otherwise.</returns>
    Task<bool> ActivateUserAsync(Guid userId);
}
