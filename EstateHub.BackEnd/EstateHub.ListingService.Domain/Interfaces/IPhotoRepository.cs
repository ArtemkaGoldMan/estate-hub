using EstateHub.ListingService.Domain.Models;

namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Repository interface for listing photo data access operations
/// </summary>
public interface IPhotoRepository
{
    /// <summary>
    /// Adds a photo to a listing
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing</param>
    /// <param name="url">The URL of the photo</param>
    /// <returns>The created photo entity</returns>
    Task<ListingPhoto> AddPhotoAsync(Guid listingId, string url);
    
    /// <summary>
    /// Removes a photo from a listing
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing</param>
    /// <param name="photoId">The unique identifier of the photo to remove</param>
    Task RemovePhotoAsync(Guid listingId, Guid photoId);
    
    /// <summary>
    /// Reorders photos for a listing
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing</param>
    /// <param name="orderedIds">Collection of photo IDs in the desired order</param>
    Task ReorderPhotosAsync(Guid listingId, IEnumerable<Guid> orderedIds);
    
    /// <summary>
    /// Gets all photos for a listing
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing</param>
    /// <returns>Collection of photos for the listing</returns>
    Task<IEnumerable<ListingPhoto>> GetPhotosByListingIdAsync(Guid listingId);
    
    /// <summary>
    /// Gets a photo by its unique identifier
    /// </summary>
    /// <param name="photoId">The unique identifier of the photo</param>
    /// <returns>The photo if found, otherwise null</returns>
    Task<ListingPhoto?> GetByIdAsync(Guid photoId);
}
