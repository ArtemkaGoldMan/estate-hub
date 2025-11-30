using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace EstateHub.ListingService.API.Controllers;

/// <summary>
/// REST API controller for photo file serving only.
/// All photo management operations (upload, delete, reorder) should be done via GraphQL.
/// Optimized for direct GridFS access and proper HTTP caching.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PhotoController : ControllerBase
{
    private readonly IPhotoService _photoService;
    private readonly MongoGridFSStorageService _gridFSService;
    private readonly ILogger<PhotoController> _logger;

    public PhotoController(
        IPhotoService photoService,
        MongoGridFSStorageService gridFSService,
        ILogger<PhotoController> logger)
    {
        _photoService = photoService;
        _gridFSService = gridFSService;
        _logger = logger;
    }

    /// <summary>
    /// Get photo by ID (serves the actual image file)
    /// This endpoint serves image files and is public for embedding images in listings.
    /// First looks up photo metadata, then serves the file from storage.
    /// </summary>
    [HttpGet("{photoId}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)] // Cache for 1 year
    public async Task<IActionResult> GetPhoto(Guid photoId)
    {
        try
        {
            var photo = await _photoService.GetPhotoAsync(photoId);
            if (photo == null)
            {
                return NotFound("Photo not found.");
            }

            // If it's a GridFS URL, use direct access for better performance
            if (photo.Url.StartsWith("/api/photo/gridfs/"))
            {
                var fileIdString = photo.Url.Split('/').Last();
                if (ObjectId.TryParse(fileIdString, out var fileId))
                {
                    return await GetPhotoFromGridFSDirect(fileId);
                }
            }

            // Try to get file stream from storage
            var streamResult = await _photoService.GetPhotoStreamAsync(photo.Url);
            if (streamResult.HasValue)
            {
                var (stream, contentType, fileName) = streamResult.Value;
                return FileWithCaching(stream, contentType, fileName, photoId.ToString());
            }

            // Fallback: redirect to the photo URL (for backward compatibility with external URLs)
            return Redirect(photo.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving photo {PhotoId}", photoId);
            return StatusCode(500, "An error occurred while retrieving the photo.");
        }
    }

    /// <summary>
    /// Get photo directly from GridFS by file ID (ObjectId)
    /// This is the primary endpoint for serving image files stored in MongoDB GridFS.
    /// Optimized for direct GridFS access without going through service layers.
    /// </summary>
    [HttpGet("gridfs/{fileId}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)] // Cache for 1 year
    public async Task<IActionResult> GetPhotoFromGridFS(string fileId)
    {
        try
        {
            if (!ObjectId.TryParse(fileId, out var objectId))
            {
                _logger.LogWarning("Invalid GridFS file ID format: {FileId}", fileId);
                return BadRequest("Invalid file ID format.");
            }

            return await GetPhotoFromGridFSDirect(objectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving photo from GridFS: FileId: {FileId}, Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                fileId, ex.GetType().Name, ex.Message, ex.StackTrace);
            return StatusCode(500, $"An error occurred while retrieving the photo: {ex.Message}");
        }
    }

    /// <summary>
    /// Internal method to retrieve photo from GridFS with caching headers
    /// </summary>
    private async Task<IActionResult> GetPhotoFromGridFSDirect(ObjectId fileId)
    {
        try
        {
            var streamResult = await _gridFSService.GetFileStreamAsync(fileId);
            if (!streamResult.HasValue)
            {
                _logger.LogWarning("Photo not found in GridFS: FileId: {FileId}", fileId);
                return NotFound("Photo not found in GridFS.");
            }

            var (stream, contentType, fileName) = streamResult.Value;
            
            // Ensure stream is at the beginning
            if (stream.CanSeek && stream.Position != 0)
            {
                stream.Position = 0;
            }
            
            return FileWithCaching(stream, contentType, fileName, fileId.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPhotoFromGridFSDirect: FileId: {FileId}, Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                fileId, ex.GetType().Name, ex.Message, ex.StackTrace);
            throw; // Re-throw to be caught by outer catch block
        }
    }

    /// <summary>
    /// Returns a FileResult with proper caching headers
    /// </summary>
    private FileStreamResult FileWithCaching(Stream stream, string contentType, string fileName, string etagValue)
    {
        // Set ETag for cache validation
        Response.Headers.ETag = $"\"{etagValue}\"";
        
        // Set cache control headers
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        
        // Use FileStreamResult which properly handles stream disposal
        // The stream will be disposed by ASP.NET Core after the response is sent
        return new FileStreamResult(stream, contentType)
        {
            FileDownloadName = fileName,
            EnableRangeProcessing = true, // Enable range requests for better performance
        };
    }
}
