using EstateHub.ListingService.Core.Abstractions;
using EstateHub.ListingService.API.Types.InputTypes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types.Mutations;

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
        var orderedPhotoIds = input.PhotoOrders.OrderBy(p => p.Order).Select(p => p.PhotoId).ToList();
        await photoService.ReorderPhotosAsync(input.ListingId, orderedPhotoIds);
        return true;
    }
}
