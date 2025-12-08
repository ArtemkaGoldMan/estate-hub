using EstateHub.ListingService.Domain.Models;

namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Repository interface for managing user-liked listings
/// </summary>
public interface ILikedListingRepository
{
    /// <summary>
    /// Gets all listings liked by a specific user
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>Collection of listings liked by the user</returns>
    Task<IEnumerable<Listing>> GetLikedByUserAsync(Guid userId);
    
    /// <summary>
    /// Adds a like relationship between a user and a listing
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="listingId">The unique identifier of the listing</param>
    Task LikeAsync(Guid userId, Guid listingId);
    
    /// <summary>
    /// Removes a like relationship between a user and a listing
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="listingId">The unique identifier of the listing</param>
    Task UnlikeAsync(Guid userId, Guid listingId);
}
