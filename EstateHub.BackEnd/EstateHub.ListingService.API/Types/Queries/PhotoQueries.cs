using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.API.Types.OutputTypes;
using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.Extensions.Logging;

namespace EstateHub.ListingService.API.Types.Queries;

[ExtendObjectType(typeof(Queries))]
public class PhotoQueries
{
    private readonly ILogger<PhotoQueries> _logger;

    public PhotoQueries(ILogger<PhotoQueries> logger)
    {
        _logger = logger;
    }

    public async Task<PhotoType?> GetPhoto(
        Guid photoId,
        [Service] IPhotoService photoService)
    {
        _logger.LogInformation("Getting photo by ID: {PhotoId}", photoId);
        var result = await photoService.GetPhotoAsync(photoId);
        _logger.LogInformation("Photo {PhotoId} found: {Found}", photoId, result != null);
        return result != null ? PhotoType.FromDto(result) : null;
    }

    public async Task<List<PhotoType>> GetPhotos(
        Guid listingId,
        [Service] IPhotoService photoService)
    {
        _logger.LogInformation("Getting photos for listing: {ListingId}", listingId);
        var result = await photoService.GetPhotosAsync(listingId);
        _logger.LogInformation("Found {Count} photos for listing {ListingId}", result.Count, listingId);
        return result.Select(PhotoType.FromDto).ToList();
    }
}
