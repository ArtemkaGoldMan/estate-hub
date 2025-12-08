using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.API.Types.OutputTypes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types.Queries;

[ExtendObjectType(typeof(Queries))]
public class ModerationQueries
{
    [Authorize]
    public async Task<ModerationResultType> CheckListingModeration(
        Guid listingId,
        [Service] IModerationService moderationService)
    {
        var result = await moderationService.CheckModerationAsync(listingId);
        return ModerationResultType.FromDto(result);
    }
}

