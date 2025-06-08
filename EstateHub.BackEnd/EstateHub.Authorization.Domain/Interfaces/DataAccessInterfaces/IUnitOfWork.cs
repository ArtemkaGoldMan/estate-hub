using CSharpFunctionalExtensions;

namespace EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;

public interface IUnitOfWork
{
    Task<Result<bool>> BeginTransactionAsync();

    Task<Result<bool>> CommitAsync();

    Task<Result<bool>> RollbackAsync();
}