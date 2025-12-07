using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.ApplicationInterfaces;
using EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.Authorization.Domain.Models;
using EstateHub.Authorization.Core.Helpers;
using EstateHub.SharedKernel;
using EstateHub.SharedKernel.Helpers;
using Microsoft.Extensions.Logging;

namespace EstateHub.Authorization.Core.Services;

/// <summary>
/// Service responsible for user management operations including retrieval, updates, deletion,
/// and administrative functions such as role management and user suspension.
/// </summary>
public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly ISessionsRepository _sessionsRepository;
    private readonly ILogger<UsersService> _logger;
    private readonly ResultExecutor<UsersService> _resultExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersService"/> class.
    /// </summary>
    /// <param name="usersRepository">Repository for user data access.</param>
    /// <param name="sessionsRepository">Repository for session data access.</param>
    /// <param name="unitOfWork">Unit of work for transaction management.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    public UsersService(
        IUsersRepository usersRepository,
        ISessionsRepository sessionsRepository,
        IUnitOfWork unitOfWork,
        ILogger<UsersService> logger)
    {
        _usersRepository = usersRepository;
        _sessionsRepository = sessionsRepository;
        _logger = logger;
        _resultExecutor = new ResultExecutor<UsersService>(_logger, unitOfWork);
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <typeparam name="TProjectTo">The DTO type to project the user data to.</typeparam>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users in the search.</param>
    /// <returns>
    /// A result containing the user DTO if found.
    /// Returns failure if ID is empty or user not found.
    /// </returns>
    public Task<Result<TProjectTo>> GetByIdAsync<TProjectTo>(Guid id, bool includeDeleted)
        where TProjectTo : class
    {
        return _resultExecutor.ExecuteAsync(
            async () =>
            {
                if (id == Guid.Empty)
                {
                    ErrorHelper.ThrowError(UserErrors.NotFoundById(id));
                }

                var user = await _usersRepository.GetByIdAsync<TProjectTo>(id, includeDeleted);
                if (user == null)
                {
                    ErrorHelper.ThrowErrorNull(UserErrors.NotFoundById(id));
                }

                return user;
            });
    }

    /// <summary>
    /// Retrieves multiple users by their unique identifiers (batch lookup).
    /// </summary>
    /// <typeparam name="TProjectTo">The DTO type to project the user data to.</typeparam>
    /// <param name="ids">List of unique identifiers of the users to retrieve.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users in the search.</param>
    /// <returns>
    /// A result containing a list of user DTOs.
    /// Returns failure if the list is empty or no users found.
    /// </returns>
    public Task<Result<List<TProjectTo>>> GetByIdsAsync<TProjectTo>(List<Guid> ids, bool includeDeleted)
        where TProjectTo : class
    {
        return _resultExecutor.ExecuteAsync(
            async () =>
            {
                if (ids.Count == 0)
                {
                    ErrorHelper.ThrowError(UserErrors.NotFoundByIds(ids));
                }

                var users = await _usersRepository.GetByIdsAsync<TProjectTo>(ids, includeDeleted);
                if (users.Count == 0)
                {
                    ErrorHelper.ThrowErrorNull(UserErrors.NotFoundByIds(ids));
                }

                return users;
            });
    }

    /// <summary>
    /// Updates user profile information including display name, avatar, contact details, and professional information.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="request">The update request containing fields to update. Empty display name will use existing value.</param>
    /// <returns>
    /// A result indicating success if the user was updated successfully.
    /// Returns failure if user not found or validation fails.
    /// </returns>
    public Task<Result> UpdateByIdAsync(Guid id, UserUpdateRequest request)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(
            async () =>
            {
                var user = await _usersRepository.GetByIdAsync<UserDto>(id);
                if (user == null)
                {
                    ErrorHelper.ThrowErrorNull(UserErrors.NotFoundById(id));
                }

                if (string.IsNullOrWhiteSpace(request.DisplayName))
                {
                    request.DisplayName = user.DisplayName;
                }

                byte[]? avatarData = null;
                string? avatarContentType = null;

                if (request.Avatar != null)
                {
                    using var memoryStream = new MemoryStream();
                    await request.Avatar.CopyToAsync(memoryStream);
                    avatarData = memoryStream.ToArray();
                    avatarContentType = request.Avatar.ContentType;
                }

                var validationResult = User.Update(
                    user.Id,
                    request.DisplayName,
                    avatarData,
                    avatarContentType);

                if (validationResult.IsFailure)
                {
                    // Parse error from Result string format
                    var errorParts = validationResult.Error.Split(TextDelimiters.Separator);
                    if (errorParts.Length >= 4)
                    {
                        var error = new Error(errorParts[0], errorParts[1], errorParts[2], errorParts[3],
                            errorParts.Length > 4 ? errorParts[4] : null);
                        ErrorHelper.ThrowError(error);
                    }
                    ErrorHelper.ThrowError(UserErrors.UpdateFailed(id));
                }

                var updateData = new UserUpdateDto
                {
                    DisplayName = validationResult.Value.DisplayName,
                    AvatarData = validationResult.Value.AvatarData,
                    AvatarContentType = validationResult.Value.AvatarContentType,
                    PhoneNumber = request.PhoneNumber,
                    Country = request.Country,
                    City = request.City,
                    Address = request.Address,
                    PostalCode = request.PostalCode,
                    CompanyName = request.CompanyName,
                    Website = request.Website
                };

                var updateResult = await _usersRepository.UpdateByIdAsync(id, updateData);
                if (!updateResult)
                {
                    ErrorHelper.ThrowError(UserErrors.UpdateFailed(id));
                }
            });
    }

    /// <summary>
    /// Soft-deletes a user account and invalidates all their sessions.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>
    /// A result indicating success if the user was deleted and sessions were invalidated.
    /// Returns failure if user not found, ID is empty, or deletion fails.
    /// </returns>
    public Task<Result> DeleteByIdAsync(Guid id)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(
            async () =>
            {
                if (id == Guid.Empty)
                {
                    ErrorHelper.ThrowError(UserErrors.NotFoundById(id));
                }

                var user = await _usersRepository.GetByIdAsync<UserDto>(id);
                if (user == null)
                {
                    ErrorHelper.ThrowErrorNull(UserErrors.NotFoundById(id));
                }

                bool userDeletionResult = await _usersRepository.DeleteByIdAsync(id);
                if (!userDeletionResult)
                {
                    ErrorHelper.ThrowError(UserErrors.DeletionFailed(id));
                }

                bool sessionsDeletionResult = await _sessionsRepository.DeleteByUserIdAsync(id);
                if (!sessionsDeletionResult)
                {
                    ErrorHelper.ThrowError(SessionErrors.DeletionFailedByUserId(id));
                }
            });
    }

    // Admin methods implementation
    
    /// <summary>
    /// Retrieves a paginated list of all users. Admin only.
    /// </summary>
    /// <typeparam name="TProjectTo">The DTO type to project the user data to.</typeparam>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users in the results.</param>
    /// <returns>
    /// A result containing a paged result with users, total count, page number, and page size.
    /// </returns>
    public Task<Result<PagedResult<TProjectTo>>> GetUsersAsync<TProjectTo>(int page, int pageSize, bool includeDeleted)
        where TProjectTo : class
    {
        return _resultExecutor.ExecuteAsync(
            async () =>
            {
                var users = await _usersRepository.GetUsersAsync<TProjectTo>(page, pageSize, includeDeleted);
                var total = await _usersRepository.GetUsersCountAsync(includeDeleted);
                
                return new PagedResult<TProjectTo>(users, total, page, pageSize);
            });
    }

    /// <summary>
    /// Retrieves user statistics including total users, active users, suspended users, and new users this month. Admin only.
    /// </summary>
    /// <returns>
    /// A result containing user statistics DTO with counts for different user states.
    /// </returns>
    public Task<Result<UserStatsDto>> GetUserStatsAsync()
    {
        return _resultExecutor.ExecuteAsync(
            async () =>
            {
                var totalUsers = await _usersRepository.GetUsersCountAsync(false);
                var activeUsers = await _usersRepository.GetActiveUsersCountAsync();
                var suspendedUsers = await _usersRepository.GetSuspendedUsersCountAsync();
                var newUsersThisMonth = await _usersRepository.GetNewUsersThisMonthCountAsync();
                
                return new UserStatsDto(totalUsers, activeUsers, suspendedUsers, newUsersThisMonth);
            });
    }

    /// <summary>
    /// Assigns a role to a user. Admin only.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="role">The role name to assign (e.g., "Admin", "User").</param>
    /// <returns>
    /// A result indicating success if the role was assigned successfully.
    /// Returns failure if user not found, ID is empty, or role assignment fails.
    /// </returns>
    public Task<Result> AssignUserRoleAsync(Guid userId, string role)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(
            async () =>
            {
                if (userId == Guid.Empty)
                {
                    ErrorHelper.ThrowError(UserErrors.NotFoundById(userId));
                }

                var success = await _usersRepository.AssignUserRoleAsync(userId, role);
                if (!success)
                {
                    ErrorHelper.ThrowError(UserErrors.UserNotAddedToRole());
                }
            });
    }

    /// <summary>
    /// Removes a role from a user. Admin only.
    /// Prevents an admin from removing their own Admin role.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="role">The role name to remove (e.g., "Admin", "User").</param>
    /// <param name="currentUserId">Optional. The ID of the current user performing the action. Used to prevent self-admin role removal.</param>
    /// <returns>
    /// A result indicating success if the role was removed successfully.
    /// Returns failure if user not found, ID is empty, attempting to remove own admin role, or role removal fails.
    /// </returns>
    public Task<Result> RemoveUserRoleAsync(Guid userId, string role, Guid? currentUserId = null)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(
            async () =>
            {
                if (userId == Guid.Empty)
                {
                    ErrorHelper.ThrowError(UserErrors.NotFoundById(userId));
                }

                // Prevent admin from removing their own Admin role
                if (currentUserId.HasValue && currentUserId.Value == userId && role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    // Check if the user actually has the Admin role
                    var user = await _usersRepository.GetByIdAsync<UserWithRolesDto>(userId);
                    if (user != null && user.Roles != null && user.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                    {
                        ErrorHelper.ThrowError(UserErrors.CannotRemoveOwnAdminRole());
                    }
                }

                var success = await _usersRepository.RemoveUserRoleAsync(userId, role);
                if (!success)
                {
                    ErrorHelper.ThrowError(UserErrors.UserNotAddedToRole());
                }
            });
    }

    /// <summary>
    /// Suspends a user account by setting a lockout period. Admin only.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to suspend.</param>
    /// <param name="reason">The reason for suspension (stored for audit purposes).</param>
    /// <returns>
    /// A result indicating success if the user was suspended successfully.
    /// Returns failure if user not found, ID is empty, or suspension fails.
    /// </returns>
    public Task<Result> SuspendUserAsync(Guid userId, string reason)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(
            async () =>
            {
                if (userId == Guid.Empty)
                {
                    ErrorHelper.ThrowError(UserErrors.NotFoundById(userId));
                }

                var success = await _usersRepository.SuspendUserAsync(userId, reason);
                if (!success)
                {
                    ErrorHelper.ThrowError(UserErrors.UpdateFailed(userId));
                }
            });
    }

    /// <summary>
    /// Activates a suspended user account by removing the lockout. Admin only.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to activate.</param>
    /// <returns>
    /// A result indicating success if the user was activated successfully.
    /// Returns failure if user not found, ID is empty, or activation fails.
    /// </returns>
    public Task<Result> ActivateUserAsync(Guid userId)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(
            async () =>
            {
                if (userId == Guid.Empty)
                {
                    ErrorHelper.ThrowError(UserErrors.NotFoundById(userId));
                }

                var success = await _usersRepository.ActivateUserAsync(userId);
                if (!success)
                {
                    ErrorHelper.ThrowError(UserErrors.UpdateFailed(userId));
                }
            });
    }

    /// <summary>
    /// Hard-deletes a user account and all associated sessions. Admin only.
    /// Admins can delete any user including themselves.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <returns>
    /// A result indicating success if the user was deleted and sessions were cleaned up.
    /// Returns failure if user not found, ID is empty, or deletion fails.
    /// </returns>
    public Task<Result> AdminDeleteUserAsync(Guid userId)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(
            async () =>
            {
                if (userId == Guid.Empty)
                {
                    ErrorHelper.ThrowError(UserErrors.NotFoundById(userId));
                }

                // Admin can delete any user (including themselves)
                var success = await _usersRepository.DeleteByIdAsync(userId);
                if (!success)
                {
                    ErrorHelper.ThrowError(UserErrors.DeletionFailed(userId));
                }

                // Also clean up sessions
                await _sessionsRepository.DeleteByUserIdAsync(userId);
            });
    }
}
