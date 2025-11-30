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

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly ISessionsRepository _sessionsRepository;
    private readonly ILogger<UsersService> _logger;
    private readonly ResultExecutor<UsersService> _resultExecutor;

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

    public Task<Result> RemoveUserRoleAsync(Guid userId, string role)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(
            async () =>
            {
                if (userId == Guid.Empty)
                {
                    ErrorHelper.ThrowError(UserErrors.NotFoundById(userId));
                }

                var success = await _usersRepository.RemoveUserRoleAsync(userId, role);
                if (!success)
                {
                    ErrorHelper.ThrowError(UserErrors.UserNotAddedToRole());
                }
            });
    }

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
