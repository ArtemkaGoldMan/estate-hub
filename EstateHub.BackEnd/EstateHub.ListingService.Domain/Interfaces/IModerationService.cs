using EstateHub.ListingService.Domain.DTO;

namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Service interface for checking listing moderation status
/// </summary>
public interface IModerationService
{
    /// <summary>
    /// Checks the moderation status of a listing
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing</param>
    /// <returns>Moderation result with approval status and suggestions if rejected</returns>
    Task<ModerationResult> CheckModerationAsync(Guid listingId);
}









