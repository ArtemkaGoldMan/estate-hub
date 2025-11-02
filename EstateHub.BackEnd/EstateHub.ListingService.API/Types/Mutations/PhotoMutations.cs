using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.API.Types.InputTypes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types.Mutations;

public class PhotoMutations
{
    /// <summary>
    /// Upload a photo file to a listing
    /// </summary>
    [Authorize]
    public async Task<Guid> UploadPhoto(
        Guid listingId,
        IFile file,
        [Service] IPhotoService photoService)
    {
        if (file == null)
        {
            throw new ArgumentException("File is required.");
        }

        await using var stream = file.OpenReadStream();
        return await photoService.UploadPhotoAsync(listingId, stream, file.Name, file.ContentType);
    }

    /// <summary>
    /// Add a photo to a listing by URL (for external images)
    /// </summary>
    [Authorize]
    public async Task<Guid> AddPhoto(
        AddPhotoInputType input,
        [Service] IPhotoService photoService)
    {
        return await photoService.AddPhotoAsync(input.ListingId, input.PhotoUrl);
    }

    /// <summary>
    /// Remove a photo from a listing
    /// </summary>
    [Authorize]
    public async Task<bool> RemovePhoto(
        Guid listingId,
        Guid photoId,
        [Service] IPhotoService photoService)
    {
        await photoService.RemovePhotoAsync(listingId, photoId);
        return true;
    }

    /// <summary>
    /// Reorder photos for a listing
    /// </summary>
    [Authorize]
    public async Task<bool> ReorderPhotos(
        ReorderPhotosInputType input,
        [Service] IPhotoService photoService)
    {
        var orderedPhotoIds = input.PhotoOrders.OrderBy(p => p.Order).Select(p => p.PhotoId).ToList();
        await photoService.ReorderPhotosAsync(input.ListingId, orderedPhotoIds);
        return true;
    }
}
