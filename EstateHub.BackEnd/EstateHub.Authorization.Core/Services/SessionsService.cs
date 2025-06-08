using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.ApplicationInterfaces;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using Microsoft.Extensions.Logging;

namespace EstateHub.Authorization.Core.Services;

public class SessionsService : ISessionsService
{
    private readonly ISessionsRepository _sessionsRepository;
    private readonly ILogger<UsersService> _logger;

    public SessionsService(ISessionsRepository sessionsRepository, ILogger<UsersService> logger)
    {
        _sessionsRepository = sessionsRepository;
        _logger = logger;
    }

    public async Task<Result<TProjectTo>> GetByIdAsync<TProjectTo>(Guid id)
        where TProjectTo : class
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning(SessionErrors.NotFound(id).ToString());
            return Result.Failure<TProjectTo>(SessionErrors.NotFound(id).ToString());
        }

        var session = await _sessionsRepository.GetByIdAsync<TProjectTo>(id);
        if (session == null)
        {
            _logger.LogWarning(SessionErrors.NotFound(id).ToString());
            return Result.Failure<TProjectTo>(SessionErrors.NotFound(id).ToString());
        }

        return Result.Success(session);
    }
}
