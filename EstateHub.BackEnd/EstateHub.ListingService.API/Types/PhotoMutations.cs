using EstateHub.ListingService.Core.Abstractions;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types;

public class PhotoMutations
{
    [Authorize]
    public async Task<Guid> AddPhoto(
        AddPhotoInputType input,
        [Service] IPhotoService photoService)
    {
        return await photoService.AddPhotoAsync(input.ListingId, input.PhotoUrl);
    }

    [Authorize]
    public async Task<bool> RemovePhoto(
        Guid listingId,
        Guid photoId,
        [Service] IPhotoService photoService)
    {
        await photoService.RemovePhotoAsync(listingId, photoId);
        return true;
    }

    [Authorize]
    public async Task<bool> ReorderPhotos(
        ReorderPhotosInputType input,
        [Service] IPhotoService photoService)
    {
        await photoService.ReorderPhotosAsync(input.ListingId, input.OrderedPhotoIds);
        return true;
    }

    // Note: UploadPhoto with IFormFile is handled via REST API controller, not GraphQL
    // GraphQL doesn't support file uploads directly - use the PhotoController instead
}
