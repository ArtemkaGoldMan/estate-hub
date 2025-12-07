using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;

namespace EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;

public interface IUsersService
{
    Task<Result<TProjectTo>> GetByIdAsync<TProjectTo>(Guid id, bool includeDeleted)
        where TProjectTo : class;

    Task<Result<List<TProjectTo>>> GetByIdsAsync<TProjectTo>(List<Guid> ids, bool includeDeleted)
        where TProjectTo : class;

    Task<Result> UpdateByIdAsync(Guid id, UserUpdateRequest request);

    Task<Result> DeleteByIdAsync(Guid id);

    // Admin methods
    Task<Result<PagedResult<TProjectTo>>> GetUsersAsync<TProjectTo>(int page, int pageSize, bool includeDeleted)
        where TProjectTo : class;

    Task<Result<UserStatsDto>> GetUserStatsAsync();

    Task<Result> AssignUserRoleAsync(Guid userId, string role);

    Task<Result> RemoveUserRoleAsync(Guid userId, string role, Guid? currentUserId = null);

    Task<Result> SuspendUserAsync(Guid userId, string reason);

    Task<Result> ActivateUserAsync(Guid userId);

    Task<Result> AdminDeleteUserAsync(Guid userId);
}

// DTOs for admin operations
public record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize);
public record UserStatsDto(int TotalUsers, int ActiveUsers, int SuspendedUsers, int NewUsersThisMonth);
