using EstateHub.ListingService.Core.Abstractions;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;

namespace EstateHub.ListingService.Core.UseCases;

public class PhotoService : IPhotoService
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IListingRepository _listingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public PhotoService(
        IPhotoRepository photoRepository,
        IListingRepository listingRepository,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _photoRepository = photoRepository;
        _listingRepository = listingRepository;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Guid> AddPhotoAsync(Guid listingId, string photoUrl)
    {
        // Verify the listing exists and user owns it
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing == null)
        {
            throw new ArgumentException($"Listing with ID {listingId} not found.");
        }

        var currentUserId = _currentUserService.GetUserId();
        if (listing.OwnerId != currentUserId)
        {
            throw new InvalidOperationException("Forbidden: You can only add photos to your own listings.");
        }

        // Validate photo URL
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            throw new ArgumentException("Photo URL cannot be empty.");
        }

        if (!Uri.IsWellFormedUriString(photoUrl, UriKind.Absolute))
        {
            throw new ArgumentException("Photo URL must be a valid absolute URL.");
        }

        var photo = await _photoRepository.AddPhotoAsync(listingId, photoUrl);
        return photo.Id;
    }

    public async Task<Guid> UploadPhotoAsync(Guid listingId, Stream fileStream, string fileName, string contentType)
    {
        // Verify the listing exists and user owns it
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing == null)
        {
            throw new ArgumentException($"Listing with ID {listingId} not found.");
        }

        var currentUserId = _currentUserService.GetUserId();
        if (listing.OwnerId != currentUserId)
        {
            throw new InvalidOperationException("Forbidden: You can only add photos to your own listings.");
        }

        // Upload file and get URL
        var photoUrl = await _fileStorageService.UploadPhotoAsync(listingId, fileStream, fileName, contentType);
        
        // Save photo URL to database
        var photo = await _photoRepository.AddPhotoAsync(listingId, photoUrl);
        return photo.Id;
    }

    public async Task RemovePhotoAsync(Guid listingId, Guid photoId)
    {
        // Verify the listing exists and user owns it
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing == null)
        {
            throw new ArgumentException($"Listing with ID {listingId} not found.");
        }

        var currentUserId = _currentUserService.GetUserId();
        if (listing.OwnerId != currentUserId)
        {
            throw new InvalidOperationException("Forbidden: You can only remove photos from your own listings.");
        }

        await _photoRepository.RemovePhotoAsync(listingId, photoId);
    }

    public async Task ReorderPhotosAsync(Guid listingId, List<Guid> orderedPhotoIds)
    {
        // Verify the listing exists and user owns it
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing == null)
        {
            throw new ArgumentException($"Listing with ID {listingId} not found.");
        }

        var currentUserId = _currentUserService.GetUserId();
        if (listing.OwnerId != currentUserId)
        {
            throw new InvalidOperationException("Forbidden: You can only reorder photos of your own listings.");
        }

        if (orderedPhotoIds == null || !orderedPhotoIds.Any())
        {
            throw new ArgumentException("Ordered photo IDs cannot be empty.");
        }

        // Verify all photos belong to this listing
        var existingPhotos = await _photoRepository.GetPhotosByListingIdAsync(listingId);
        var existingPhotoIds = existingPhotos.Select(p => p.Id).ToHashSet();
        
        var invalidPhotoIds = orderedPhotoIds.Where(id => !existingPhotoIds.Contains(id)).ToList();
        if (invalidPhotoIds.Any())
        {
            throw new ArgumentException($"Photos with IDs {string.Join(", ", invalidPhotoIds)} do not belong to this listing.");
        }

        await _photoRepository.ReorderPhotosAsync(listingId, orderedPhotoIds);
    }

    public async Task<List<PhotoDto>> GetPhotosAsync(Guid listingId)
    {
        var photos = await _photoRepository.GetPhotosByListingIdAsync(listingId);
        return photos.Select(MapToDto).ToList();
    }

    public async Task<PhotoDto?> GetPhotoAsync(Guid photoId)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId);
        return photo != null ? MapToDto(photo) : null;
    }

    private static PhotoDto MapToDto(ListingPhoto photo) => new PhotoDto(
        photo.Id,
        photo.ListingId,
        photo.Url,
        photo.Order
    );
}
