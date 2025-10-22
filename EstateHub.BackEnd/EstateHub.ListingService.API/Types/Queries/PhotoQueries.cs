using EstateHub.ListingService.Core.Abstractions;
using EstateHub.ListingService.API.Types.OutputTypes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types.Queries;

public class PhotoQueries
{
    [Authorize]
    public async Task<PhotoType?> GetPhoto(
        Guid photoId,
        [Service] IPhotoService photoService)
    {
        var result = await photoService.GetPhotoAsync(photoId);
        return result != null ? PhotoType.FromDto(result) : null;
    }

    [Authorize]
    public async Task<List<PhotoType>> GetPhotos(
        Guid listingId,
        [Service] IPhotoService photoService)
    {
        var result = await photoService.GetPhotosAsync(listingId);
        return result.Select(PhotoType.FromDto).ToList();
    }
}
