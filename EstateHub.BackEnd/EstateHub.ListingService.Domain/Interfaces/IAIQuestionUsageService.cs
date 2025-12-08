namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Service interface for managing AI question usage limits per user
/// </summary>
public interface IAIQuestionUsageService
{
    /// <summary>
    /// Checks if user can ask a question and increments usage count if allowed
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>Tuple containing whether user can ask and remaining question count</returns>
    Task<(bool CanAsk, int RemainingCount)> CheckAndIncrementUsageAsync(Guid userId);
    
    /// <summary>
    /// Gets the remaining question count for a user without incrementing
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>Number of remaining questions the user can ask today</returns>
    Task<int> GetRemainingCountAsync(Guid userId);
}


