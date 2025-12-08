using EstateHub.Authorization.Domain.Models;

namespace EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;

/// <summary>
/// Repository interface for session data access operations.
/// Provides methods for managing user sessions and refresh tokens in the data store.
/// </summary>
public interface ISessionsRepository
{
    /// <summary>
    /// Retrieves a session by its refresh token.
    /// </summary>
    /// <typeparam name="TProjectTo">The type to project the session data to (e.g., SessionDto).</typeparam>
    /// <param name="refreshToken">The refresh token associated with the session.</param>
    /// <returns>The projected session data if found, or null if the session doesn't exist or the token is invalid.</returns>
    Task<TProjectTo?> GetByRefreshTokenAsync<TProjectTo>(string refreshToken)
        where TProjectTo : class;
    
    /// <summary>
    /// Retrieves a session by its unique identifier.
    /// </summary>
    /// <typeparam name="TProjectTo">The type to project the session data to.</typeparam>
    /// <param name="id">The unique identifier of the session.</param>
    /// <returns>The projected session data if found, or null if the session doesn't exist.</returns>
    Task<TProjectTo?> GetByIdAsync<TProjectTo>(Guid id)
        where TProjectTo : class;

    /// <summary>
    /// Creates a new session in the data store.
    /// </summary>
    /// <typeparam name="TProjectTo">The type to project the created session data to.</typeparam>
    /// <param name="session">The session model to create.</param>
    /// <returns>The projected session data of the created session.</returns>
    Task<TProjectTo> CreateAsync<TProjectTo>(Session session);

    /// <summary>
    /// Updates an existing session in the data store.
    /// </summary>
    /// <param name="session">The session model containing updated information.</param>
    /// <returns>True if the update was successful, false otherwise.</returns>
    Task<bool> UpdateAsync(Session session);

    /// <summary>
    /// Deletes a session by its refresh token.
    /// Used for logout operations to invalidate a specific session.
    /// </summary>
    /// <param name="refreshToken">The refresh token associated with the session to delete.</param>
    /// <returns>True if the deletion was successful, false otherwise.</returns>
    Task<bool> DeleteAsync(string refreshToken);

    /// <summary>
    /// Deletes all sessions for a specific user.
    /// Used for logout from all devices or account security operations.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose sessions should be deleted.</param>
    /// <returns>True if the deletion was successful, false otherwise.</returns>
    Task<bool> DeleteByUserIdAsync(Guid userId);
}