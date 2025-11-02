using EstateHub.ListingService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstateHub.ListingService.API.Controllers;

/// <summary>
/// REST API controller for photo file serving only.
/// All photo management operations (upload, delete, reorder) should be done via GraphQL.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PhotoController : ControllerBase
{
    private readonly IPhotoService _photoService;
    private readonly ILogger<PhotoController> _logger;

    public PhotoController(
        IPhotoService photoService,
        ILogger<PhotoController> logger)
    {
        _photoService = photoService;
        _logger = logger;
    }

    /// <summary>
    /// Get photo by ID (serves the actual image file)
    /// This endpoint serves image files and is public for embedding images in listings
    /// </summary>
    [HttpGet("{photoId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPhoto(Guid photoId)
    {
        try
        {
            var photo = await _photoService.GetPhotoAsync(photoId);
            if (photo == null)
            {
                return NotFound("Photo not found.");
            }

            // Try to get file stream from storage
            var streamResult = await _photoService.GetPhotoStreamAsync(photo.Url);
            if (streamResult.HasValue)
            {
                var (stream, contentType, fileName) = streamResult.Value;
                return File(stream, contentType, fileName);
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
    /// Get photo directly from GridFS by file ID
    /// This is the primary endpoint for serving image files stored in MongoDB GridFS
    /// </summary>
    [HttpGet("gridfs/{fileId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPhotoFromGridFS(string fileId)
    {
        try
        {
            // Build GridFS URL format
            var photoUrl = $"/api/photo/gridfs/{fileId}";
            
            var streamResult = await _photoService.GetPhotoStreamAsync(photoUrl);
            if (!streamResult.HasValue)
            {
                return NotFound("Photo not found in GridFS.");
            }

            var (stream, contentType, fileName) = streamResult.Value;
            return File(stream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving photo from GridFS: FileId: {FileId}", fileId);
            return StatusCode(500, "An error occurred while retrieving the photo.");
        }
    }
}
