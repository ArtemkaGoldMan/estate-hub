using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.Models;

namespace EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;

public interface IUsersRepository
{
    Task<TProjectTo?> GetByIdAsync<TProjectTo>(Guid id, bool includeDeleted = false)
        where TProjectTo : class;

    Task<TProjectTo?> GetByEmailAsync<TProjectTo>(string email, bool includeDeleted = false)
        where TProjectTo : class;

    Task<List<TProjectTo>> GetByIdsAsync<TProjectTo>(List<Guid> ids, bool includeDeleted = false)
        where TProjectTo : class;

    Task<bool> UpdateByIdAsync(Guid id, UserUpdateDto user);
    Task<bool> DeleteByIdAsync(Guid id);
}
