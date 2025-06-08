using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using Microsoft.Extensions.Logging;

namespace EstateHub.Authorization.Core.Helpers;

public class ResultExecutor<TLogger>
{
    private readonly ILogger<TLogger> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ResultExecutor(ILogger<TLogger> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TResult>> ExecuteWithTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        bool beginTransaction = true)
    {
        try
        {
            if (beginTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
            }

            var result = await operation();

            if (beginTransaction)
            {
                await _unitOfWork.CommitAsync();
            }

            return Result.Success(result);
        }
        catch (Exception e) when (e is ArgumentNullException or ArgumentException or AggregateException)
        {
            if (beginTransaction)
            {
                await _unitOfWork.RollbackAsync();
            }

            _logger.LogError("{error}", e.Message);
            return Result.Failure<TResult>(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("{error}", e.Message);

            if (beginTransaction)
            {
                await _unitOfWork.RollbackAsync();
            }

            throw;
        }
    }

    public async Task<Result> ExecuteWithTransactionAsync(
        Func<Task> operation,
        bool beginTransaction = true)
    {
        try
        {
            if (beginTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
            }

            await operation();

            if (beginTransaction)
            {
                await _unitOfWork.CommitAsync();
            }

            return Result.Success();
        }
        catch (Exception e) when (e is ArgumentNullException or ArgumentException or AggregateException)
        {
            if (beginTransaction)
            {
                await _unitOfWork.RollbackAsync();
            }

            _logger.LogError("{error}", e.Message);
            return Result.Failure(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("{error}", e.Message);

            if (beginTransaction)
            {
                await _unitOfWork.RollbackAsync();
            }

            throw;
        }
    }

    public async Task<Result<TResult>> ExecuteAsync<TResult>(Func<Task<TResult>> operation)
    {
        try
        {
            var result = await operation();
            return Result.Success(result);
        }
        catch (Exception e) when (e is ArgumentNullException or ArgumentException or AggregateException)
        {
            _logger.LogError("{error}", e.Message);
            return Result.Failure<TResult>(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("{error}", e.Message);
            throw;
        }
    }

    public async Task<Result> ExecuteAsync(Func<Task> operation)
    {
        try
        {
            await operation();
            return Result.Success();
        }
        catch (Exception e) when (e is ArgumentNullException or ArgumentException or AggregateException)
        {
            _logger.LogError("{error}", e.Message);
            return Result.Failure(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("{error}", e.Message);
            throw;
        }
    }
}
