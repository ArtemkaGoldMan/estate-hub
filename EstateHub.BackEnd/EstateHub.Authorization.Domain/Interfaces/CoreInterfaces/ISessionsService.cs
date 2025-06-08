using CSharpFunctionalExtensions;

namespace EstateHub.Authorization.Domain.Interfaces.ApplicationInterfaces;

public interface ISessionsService
{
    Task<Result<TProjectTo>> GetByIdAsync<TProjectTo>(Guid id)
        where TProjectTo : class;
}
