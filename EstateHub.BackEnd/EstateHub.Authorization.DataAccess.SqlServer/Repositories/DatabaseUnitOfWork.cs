using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace EstateHub.Authorization.DataAccess.SqlServer.Repositories;

public class DatabaseUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;
    private bool _disposed = false;

    public DatabaseUnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result<bool>> BeginTransactionAsync()
    {
        try
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return Result.Success(true);
        }
        catch (Exception e)
        {
            return Result.Failure<bool>($"Failed to begin transaction: {e.Message}");
        }
    }

    public async Task<Result<bool>> CommitAsync()
    {
        if (_transaction == null)
        {
            return Result.Failure<bool>("No active transaction to commit");
        }

        try
        {
            await _transaction.CommitAsync();
            return Result.Success(true);
        }
        catch (Exception e)
        {
            await RollbackInternalAsync();
            return Result.Failure<bool>($"Error during commit: {e.Message}");
        }
    }

    public async Task<Result<bool>> RollbackAsync()
    {
        if (_transaction == null)
        {
            return Result.Failure<bool>("No active transaction to roll back");
        }

        try
        {
            await RollbackInternalAsync();
            return Result.Success(true);
        }
        catch (Exception e)
        {
            return Result.Failure<bool>($"Error during rollback: {e.Message}");
        }
    }

    // Internal method to avoid code duplication
    private async Task RollbackInternalAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
            }

            _transaction = null;
            _disposed = true;
        }
    }

    ~DatabaseUnitOfWork()
    {
        Dispose(false);
    }
}
