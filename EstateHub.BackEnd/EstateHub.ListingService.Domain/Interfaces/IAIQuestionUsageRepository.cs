namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Repository interface for managing AI question usage tracking per user
/// </summary>
public interface IAIQuestionUsageRepository
{
    /// <summary>
    /// Gets the number of AI questions asked by the user today
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>The count of questions asked today by the user</returns>
    Task<int> GetTodayQuestionCountAsync(Guid userId);
    
    /// <summary>
    /// Increments the question count for the user for today
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    Task IncrementQuestionCountAsync(Guid userId);
}


