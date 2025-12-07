namespace EstateHub.ListingService.Domain.Interfaces;

public interface IAIQuestionUsageService
{
    Task<(bool CanAsk, int RemainingCount)> CheckAndIncrementUsageAsync(Guid userId);
    Task<int> GetRemainingCountAsync(Guid userId);
}

