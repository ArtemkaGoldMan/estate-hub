using EstateHub.ListingService.Domain.DTO;

namespace EstateHub.ListingService.Domain.Interfaces;

public interface IModerationService
{
    Task<ModerationResult> CheckModerationAsync(Guid listingId);
}




