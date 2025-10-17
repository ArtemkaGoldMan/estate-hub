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

    [Authorize]
    public async Task<Guid> UploadPhoto(
        Guid listingId,
        IFormFile file,
        [Service] IPhotoService photoService)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file uploaded.");
        }

        using var stream = file.OpenReadStream();
        return await photoService.UploadPhotoAsync(listingId, stream, file.FileName, file.ContentType);
    }
}
