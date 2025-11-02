using EstateHub.ListingService.Domain.DTO;

namespace EstateHub.ListingService.Domain.Interfaces;

public interface IPhotoService
{
    /// <summary>
    /// Add a photo to a listing by URL
    /// </summary>
    Task<Guid> AddPhotoAsync(Guid listingId, string photoUrl);
    
    /// <summary>
    /// Upload a photo file to a listing
    /// </summary>
    Task<Guid> UploadPhotoAsync(Guid listingId, Stream fileStream, string fileName, string contentType);
    
    /// <summary>
    /// Remove a photo from a listing
    /// </summary>
    Task RemovePhotoAsync(Guid listingId, Guid photoId);
    
    /// <summary>
    /// Reorder photos for a listing
    /// </summary>
    Task ReorderPhotosAsync(Guid listingId, List<Guid> orderedPhotoIds);
    
    /// <summary>
    /// Get all photos for a listing
    /// </summary>
    Task<List<PhotoDto>> GetPhotosAsync(Guid listingId);
    
    /// <summary>
    /// Get a specific photo by ID
    /// </summary>
    Task<PhotoDto?> GetPhotoAsync(Guid photoId);
    
    /// <summary>
    /// Get file stream for serving a photo file
    /// </summary>
    Task<(Stream Stream, string ContentType, string FileName)?> GetPhotoStreamAsync(string photoUrl);
}

