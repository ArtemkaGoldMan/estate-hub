using EstateHub.ListingService.Domain.Models;

namespace EstateHub.ListingService.Domain.Interfaces;

public interface ILikedListingRepository
{
    Task<IEnumerable<Listing>> GetLikedByUserAsync(Guid userId);
    Task LikeAsync(Guid userId, Guid listingId);
    Task UnlikeAsync(Guid userId, Guid listingId);
}
