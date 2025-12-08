using CSharpFunctionalExtensions;

namespace EstateHub.Authorization.Domain.Interfaces.ApplicationInterfaces;

/// <summary>
/// Service interface for session management operations.
/// Provides methods for retrieving session information.
/// </summary>
public interface ISessionsService
{
    /// <summary>
    /// Retrieves a session by its unique identifier.
    /// </summary>
    /// <typeparam name="TProjectTo">The type to project the session data to (e.g., SessionDto).</typeparam>
    /// <param name="id">The unique identifier of the session.</param>
    /// <returns>A Result containing the projected session data if found, or an error if the session doesn't exist.</returns>
    Task<Result<TProjectTo>> GetByIdAsync<TProjectTo>(Guid id)
        where TProjectTo : class;
}
