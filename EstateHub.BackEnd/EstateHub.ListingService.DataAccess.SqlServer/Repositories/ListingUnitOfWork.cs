using CSharpFunctionalExtensions;
using EstateHub.ListingService.DataAccess.SqlServer.Db;
using EstateHub.SharedKernel.Execution;
using Microsoft.EntityFrameworkCore.Storage;

namespace EstateHub.ListingService.DataAccess.SqlServer.Repositories;

public class ListingUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    public ListingUnitOfWork(ApplicationDbContext context)
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

    private async Task RollbackInternalAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_transaction != null)
            {
                await RollbackInternalAsync();
            }
            _disposed = true;
        }
    }
}

