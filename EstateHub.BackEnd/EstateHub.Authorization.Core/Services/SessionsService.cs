using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.ApplicationInterfaces;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.Authorization.Core.Helpers;
using EstateHub.SharedKernel.Helpers;
using Microsoft.Extensions.Logging;

namespace EstateHub.Authorization.Core.Services;

public class SessionsService : ISessionsService
{
    private readonly ISessionsRepository _sessionsRepository;
    private readonly ILogger<SessionsService> _logger;
    private readonly ResultExecutor<SessionsService> _resultExecutor;

    public SessionsService(
        ISessionsRepository sessionsRepository,
        IUnitOfWork unitOfWork,
        ILogger<SessionsService> logger)
    {
        _sessionsRepository = sessionsRepository;
        _logger = logger;
        _resultExecutor = new ResultExecutor<SessionsService>(_logger, unitOfWork);
    }

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
