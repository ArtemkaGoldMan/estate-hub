using CSharpFunctionalExtensions;

namespace EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;

/// <summary>
/// Unit of Work pattern interface for managing database transactions.
/// Provides methods to begin, commit, and rollback database transactions to ensure data consistency.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Begins a new database transaction.
    /// All subsequent operations will be part of this transaction until committed or rolled back.
    /// </summary>
    /// <returns>A Result indicating whether the transaction was successfully started.</returns>
    Task<Result<bool>> BeginTransactionAsync();

    /// <summary>
    /// Commits the current database transaction.
    /// All changes made within the transaction will be persisted to the database.
    /// </summary>
    /// <returns>A Result indicating whether the commit was successful.</returns>
    Task<Result<bool>> CommitAsync();

    /// <summary>
    /// Rolls back the current database transaction.
    /// All changes made within the transaction will be discarded.
    /// </summary>
    /// <returns>A Result indicating whether the rollback was successful.</returns>
    Task<Result<bool>> RollbackAsync();
}