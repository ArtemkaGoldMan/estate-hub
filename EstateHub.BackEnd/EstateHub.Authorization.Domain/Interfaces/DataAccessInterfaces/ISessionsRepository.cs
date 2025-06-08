using EstateHub.Authorization.Domain.Models;

namespace EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;

public interface ISessionsRepository
{
    Task<TProjectTo?> GetByRefreshTokenAsync<TProjectTo>(string refreshToken)
        where TProjectTo : class;
    
    Task<TProjectTo?> GetByIdAsync<TProjectTo>(Guid id)
        where TProjectTo : class;

    Task<TProjectTo> CreateAsync<TProjectTo>(Session session);
    Task<bool> UpdateAsync(Session session);

    Task<bool> DeleteAsync(string refreshToken);
    Task<bool> DeleteByUserIdAsync(Guid userId);
}