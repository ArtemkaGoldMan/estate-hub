using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.ApplicationInterfaces;
using EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.Authorization.Domain.Models;
using EstateHub.Authorization.Core.Helpers;
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
                    throw new ArgumentException(UserErrors.NotFoundById(id).ToString());
                }

                var user = await _usersRepository.GetByIdAsync<TProjectTo>(id, includeDeleted);
                if (user == null)
                {
                    throw new ArgumentNullException(UserErrors.NotFoundById(id).ToString());
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
                    throw new ArgumentException(UserErrors.NotFoundByIds(ids).ToString());
                }

                var users = await _usersRepository.GetByIdsAsync<TProjectTo>(ids, includeDeleted);
                if (users.Count == 0)
                {
                    throw new ArgumentNullException(UserErrors.NotFoundByIds(ids).ToString());
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
                    throw new ArgumentNullException(UserErrors.NotFoundById(id).ToString());
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
                    throw new ArgumentException(validationResult.Error);
                }

                var updateData = new UserUpdateDto
                {
                    DisplayName = validationResult.Value.DisplayName,
                    AvatarData = validationResult.Value.AvatarData,
                    AvatarContentType = validationResult.Value.AvatarContentType
                };

                var updateResult = await _usersRepository.UpdateByIdAsync(id, updateData);
                if (!updateResult)
                {
                    throw new ArgumentException(UserErrors.UpdateFailed(id).ToString());
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
                    throw new ArgumentException(UserErrors.NotFoundById(id).ToString());
                }

                var user = await _usersRepository.GetByIdAsync<UserDto>(id);
                if (user == null)
                {
                    throw new ArgumentNullException(UserErrors.NotFoundById(id).ToString());
                }

                bool userDeletionResult = await _usersRepository.DeleteByIdAsync(id);
                if (!userDeletionResult)
                {
                    throw new ArgumentException(UserErrors.DeletionFailed(id).ToString());
                }

                bool sessionsDeletionResult = await _sessionsRepository.DeleteByUserIdAsync(id);
                if (!sessionsDeletionResult)
                {
                    throw new ArgumentException(SessionErrors.DeletionFailedByUserId(id).ToString());
                }
            });
    }
}
