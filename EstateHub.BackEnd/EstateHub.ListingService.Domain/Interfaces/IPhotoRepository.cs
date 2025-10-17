using EstateHub.ListingService.Domain.Models;

namespace EstateHub.ListingService.Domain.Interfaces;

public interface IPhotoRepository
{
    Task<ListingPhoto> AddPhotoAsync(Guid listingId, string url);
    Task RemovePhotoAsync(Guid listingId, Guid photoId);
    Task ReorderPhotosAsync(Guid listingId, IEnumerable<Guid> orderedIds);
    Task<IEnumerable<ListingPhoto>> GetPhotosByListingIdAsync(Guid listingId);
    Task<ListingPhoto?> GetByIdAsync(Guid photoId);
}
