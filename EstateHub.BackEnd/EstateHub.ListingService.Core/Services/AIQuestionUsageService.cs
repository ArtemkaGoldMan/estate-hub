using EstateHub.ListingService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EstateHub.ListingService.Core.Services;

public class AIQuestionUsageService : IAIQuestionUsageService
{
    private const int DailyLimit = 5;
    private readonly IAIQuestionUsageRepository _repository;
    private readonly ILogger<AIQuestionUsageService> _logger;

    public AIQuestionUsageService(
        IAIQuestionUsageRepository repository,
        ILogger<AIQuestionUsageService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<(bool CanAsk, int RemainingCount)> CheckAndIncrementUsageAsync(Guid userId)
    {
        var currentCount = await _repository.GetTodayQuestionCountAsync(userId);
        var remainingCount = Math.Max(0, DailyLimit - currentCount);
        
        if (currentCount >= DailyLimit)
        {
            _logger.LogWarning("User {UserId} has reached daily AI question limit ({Limit})", userId, DailyLimit);
            return (false, 0);
        }

        await _repository.IncrementQuestionCountAsync(userId);
        _logger.LogInformation("User {UserId} asked AI question. Count: {Count}/{Limit}", userId, currentCount + 1, DailyLimit);
        
        return (true, remainingCount - 1);
    }

    public async Task<int> GetRemainingCountAsync(Guid userId)
    {
        var currentCount = await _repository.GetTodayQuestionCountAsync(userId);
        return Math.Max(0, DailyLimit - currentCount);
    }
}

