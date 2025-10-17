using EstateHub.ListingService.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstateHub.ListingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PhotoController : ControllerBase
{
    private readonly IPhotoService _photoService;
    private readonly ILogger<PhotoController> _logger;

    public PhotoController(IPhotoService photoService, ILogger<PhotoController> logger)
    {
        _photoService = photoService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a photo for a listing
    /// </summary>
    [HttpPost("upload/{listingId}")]
    public async Task<IActionResult> UploadPhoto(Guid listingId, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            _logger.LogInformation("Uploading photo for listing {ListingId}, file: {FileName}, size: {Size} bytes", 
                listingId, file.FileName, file.Length);

            using var stream = file.OpenReadStream();
            var photoId = await _photoService.UploadPhotoAsync(listingId, stream, file.FileName, file.ContentType);

            return Ok(new { PhotoId = photoId, Message = "Photo uploaded successfully." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid photo upload request for listing {ListingId}", listingId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized photo upload attempt for listing {ListingId}", listingId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for listing {ListingId}", listingId);
            return StatusCode(500, "An error occurred while uploading the photo.");
        }
    }

    /// <summary>
    /// Get photo by ID (serves the actual image file)
    /// </summary>
    [HttpGet("{photoId}")]
    public async Task<IActionResult> GetPhoto(Guid photoId)
    {
        try
        {
            var photo = await _photoService.GetPhotoAsync(photoId);
            if (photo == null)
            {
                return NotFound("Photo not found.");
            }

            // Redirect to the photo URL (for local storage, this will be a relative path)
            return Redirect(photo.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving photo {PhotoId}", photoId);
            return StatusCode(500, "An error occurred while retrieving the photo.");
        }
    }
}
