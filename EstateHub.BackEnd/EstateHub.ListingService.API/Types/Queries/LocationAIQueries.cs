using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Core.Services;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types.Queries;

[ExtendObjectType(typeof(Queries))]
public class LocationAIQueries
{
    [Authorize]
    public async Task<AskAboutLocationResult> AskAboutLocation(
        Guid listingId,
        string questionId,
        [Service] IListingService listingService,
        [Service] ILocationAIService locationAIService,
        [Service] IAIQuestionUsageService usageService,
        [Service] ICurrentUserService currentUserService)
    {
        var userId = currentUserService.GetUserId();
        if (userId == Guid.Empty)
        {
            throw new GraphQLException("User not authenticated");
        }

        var (canAsk, remainingCount) = await usageService.CheckAndIncrementUsageAsync(userId);
        if (!canAsk)
        {
            throw new GraphQLException($"Daily limit reached. You have used all 5 questions for today. Please try again tomorrow.");
        }

        var listing = await listingService.GetByIdAsync(listingId);
        if (listing == null)
        {
            throw new GraphQLException("Listing not found");
        }

        if (!AIQuestionPromptMapper.IsValidQuestionId(questionId))
        {
            throw new GraphQLException($"Invalid question ID: {questionId}");
        }
        
        var detailedPrompt = AIQuestionPromptMapper.GetPromptForQuestion(questionId);
        var answer = await locationAIService.AskAboutLocationAsync(
            detailedPrompt,
            listing.City,
            listing.District,
            listing.Latitude,
            listing.Longitude);

        return new AskAboutLocationResult
        {
            Answer = answer,
            RemainingQuestions = remainingCount
        };
    }

    [Authorize]
    [GraphQLName("getRemainingAIQuestions")]
    public async Task<int> GetRemainingAIQuestions(
        [Service] IAIQuestionUsageService usageService,
        [Service] ICurrentUserService currentUserService)
    {
        var userId = currentUserService.GetUserId();
        if (userId == Guid.Empty)
        {
            return 0;
        }

        return await usageService.GetRemainingCountAsync(userId);
    }
}

public class AskAboutLocationResult
{
    public string Answer { get; set; } = string.Empty;
    public int RemainingQuestions { get; set; }
}

