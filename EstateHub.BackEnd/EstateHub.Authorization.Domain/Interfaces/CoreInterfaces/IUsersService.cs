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
}
