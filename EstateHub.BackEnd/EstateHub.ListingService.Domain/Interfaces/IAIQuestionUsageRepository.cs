namespace EstateHub.ListingService.Domain.Interfaces;

public interface IAIQuestionUsageRepository
{
    Task<int> GetTodayQuestionCountAsync(Guid userId);
    Task IncrementQuestionCountAsync(Guid userId);
}

