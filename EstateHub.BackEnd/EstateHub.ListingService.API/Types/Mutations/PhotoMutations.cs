using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.API.Types.InputTypes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types.Mutations;

/// <summary>
/// GraphQL mutations for listing photo operations.
/// Extends the base Mutations type with photo management functionality.
/// </summary>
[ExtendObjectType(typeof(Mutations))]
public class PhotoMutations
{
    /// <summary>
    /// Uploads a photo file to a listing. Requires authentication.
    /// The photo file is stored and associated with the specified listing.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing to add the photo to.</param>
    /// <param name="file">The photo file to upload (supports standard image formats).</param>
    /// <param name="photoService">The photo service injected by HotChocolate.</param>
    /// <returns>The unique identifier (Guid) of the newly uploaded photo.</returns>
    /// <exception cref="ArgumentException">Thrown when the file parameter is null.</exception>
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
        return await photoService.UploadPhotoAsync(
            listingId, 
            stream, 
            file.Name ?? "photo", 
            file.ContentType ?? "application/octet-stream");
    }

    /// <summary>
    /// Adds a photo to a listing by URL (for external images). Requires authentication.
    /// Useful for adding photos from external sources or URLs.
    /// </summary>
    /// <param name="input">The add photo input containing the listing ID and photo URL.</param>
    /// <param name="photoService">The photo service injected by HotChocolate.</param>
    /// <returns>The unique identifier (Guid) of the newly added photo.</returns>
    [Authorize]
    public async Task<Guid> AddPhoto(
        AddPhotoInputType input,
        [Service] IPhotoService photoService)
    {
        return await photoService.AddPhotoAsync(input.ListingId, input.PhotoUrl);
    }

    /// <summary>
    /// Removes a photo from a listing. Requires authentication.
    /// Users can only remove photos from their own listings.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    /// <param name="photoId">The unique identifier of the photo to remove.</param>
    /// <param name="photoService">The photo service injected by HotChocolate.</param>
    /// <returns>True if the photo was successfully removed.</returns>
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
    /// Reorders photos for a listing. Requires authentication.
    /// Updates the display order of photos based on the provided order values.
    /// Users can only reorder photos for their own listings.
    /// </summary>
    /// <param name="input">The reorder photos input containing the listing ID and photo orders.</param>
    /// <param name="photoService">The photo service injected by HotChocolate.</param>
    /// <returns>True if the photos were successfully reordered.</returns>
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
