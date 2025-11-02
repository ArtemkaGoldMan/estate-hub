using EstateHub.ListingService.Domain.DTO;

namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Interface for photo file storage operations.
/// Minimal abstraction for file upload, delete, and streaming.
/// </summary>
public interface IPhotoStorageService
{
    /// <summary>
    /// Upload a photo file and return the URL
    /// </summary>
    Task<string> UploadPhotoAsync(Guid listingId, Stream fileStream, string fileName, string contentType);
    
    /// <summary>
    /// Delete a photo file by URL
    /// </summary>
    Task DeletePhotoAsync(string photoUrl);
    
    /// <summary>
    /// Get file stream for serving a photo file
    /// </summary>
    Task<(Stream Stream, string ContentType, string FileName)?> GetPhotoStreamAsync(string photoUrl);
    
    /// <summary>
    /// Validate file before upload
    /// </summary>
    Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string contentType);
}

