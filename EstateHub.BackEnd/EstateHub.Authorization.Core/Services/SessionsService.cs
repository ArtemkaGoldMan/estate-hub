using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.ApplicationInterfaces;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.Authorization.Core.Helpers;
using EstateHub.SharedKernel.Helpers;
using Microsoft.Extensions.Logging;

namespace EstateHub.Authorization.Core.Services;

/// <summary>
/// Service responsible for session management operations including session retrieval.
/// </summary>
public class SessionsService : ISessionsService
{
    private readonly ISessionsRepository _sessionsRepository;
    private readonly ILogger<SessionsService> _logger;
    private readonly ResultExecutor<SessionsService> _resultExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionsService"/> class.
    /// </summary>
    /// <param name="sessionsRepository">Repository for session data access.</param>
    /// <param name="unitOfWork">Unit of work for transaction management.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    public SessionsService(
        ISessionsRepository sessionsRepository,
        IUnitOfWork unitOfWork,
        ILogger<SessionsService> logger)
    {
        _sessionsRepository = sessionsRepository;
        _logger = logger;
        _resultExecutor = new ResultExecutor<SessionsService>(_logger, unitOfWork);
    }

    /// <summary>
    /// Retrieves a session by its unique identifier.
    /// </summary>
    /// <typeparam name="TProjectTo">The DTO type to project the session data to.</typeparam>
    /// <param name="id">The unique identifier of the session.</param>
    /// <returns>
    /// A result containing the session DTO if found.
    /// Returns failure if ID is empty or session not found.
    /// </returns>
    public Task<Result<TProjectTo>> GetByIdAsync<TProjectTo>(Guid id)
        where TProjectTo : class
    {
        return _resultExecutor.ExecuteAsync(async () =>
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning(SessionErrors.NotFound(id).ToString());
                ErrorHelper.ThrowError(SessionErrors.NotFound(id));
            }

            var session = await _sessionsRepository.GetByIdAsync<TProjectTo>(id);
            if (session == null)
            {
                _logger.LogWarning(SessionErrors.NotFound(id).ToString());
                ErrorHelper.ThrowErrorNull(SessionErrors.NotFound(id));
            }

            return session;
        });
    }
}
