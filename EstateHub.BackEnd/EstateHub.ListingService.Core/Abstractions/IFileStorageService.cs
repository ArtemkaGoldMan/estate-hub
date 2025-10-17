namespace EstateHub.ListingService.Core.Abstractions;

public interface IFileStorageService
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
    /// Get the full URL for a photo
    /// </summary>
    string GetPhotoUrl(string relativePath);
    
    /// <summary>
    /// Validate file before upload
    /// </summary>
    Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string contentType);
}

public record FileValidationResult(
    bool IsValid,
    string? ErrorMessage = null,
    long FileSize = 0,
    string? DetectedContentType = null
);
