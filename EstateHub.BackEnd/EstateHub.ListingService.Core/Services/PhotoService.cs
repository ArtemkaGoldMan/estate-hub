using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Models;
using EstateHub.ListingService.Domain.Errors;
using EstateHub.SharedKernel;
using EstateHub.SharedKernel.Execution;
using EstateHub.SharedKernel.Helpers;
using Microsoft.Extensions.Logging;

namespace EstateHub.ListingService.Core.Services;

public class PhotoService : IPhotoService
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IListingRepository _listingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPhotoStorageService _photoStorageService;
    private readonly ResultExecutor<PhotoService> _resultExecutor;

    public PhotoService(
        IPhotoRepository photoRepository,
        IListingRepository listingRepository,
        ICurrentUserService currentUserService,
        IPhotoStorageService photoStorageService,
        IUnitOfWork unitOfWork,
        ILogger<PhotoService> logger)
    {
        _photoRepository = photoRepository;
        _listingRepository = listingRepository;
        _currentUserService = currentUserService;
        _photoStorageService = photoStorageService;
        _resultExecutor = new ResultExecutor<PhotoService>(logger, unitOfWork);
    }

    public async Task<Guid> AddPhotoAsync(Guid listingId, string photoUrl)
    {
        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            // Verify the listing exists and user owns it
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null)
            {
                ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(listingId));
            }

            var currentUserId = _currentUserService.GetUserId();
            if (listing.OwnerId != currentUserId)
            {
                ErrorHelper.ThrowErrorOperation(ListingServiceErrors.NotOwner());
            }

            // Validate photo URL
            if (string.IsNullOrWhiteSpace(photoUrl))
            {
                ErrorHelper.ThrowError(ListingServiceErrors.PhotoUrlEmpty());
            }

            if (!Uri.IsWellFormedUriString(photoUrl, UriKind.Absolute))
            {
                ErrorHelper.ThrowError(ListingServiceErrors.PhotoUrlInvalid());
            }

            var photo = await _photoRepository.AddPhotoAsync(listingId, photoUrl);
            return photo.Id;
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }

        return result.Value;
    }

    public async Task<Guid> UploadPhotoAsync(Guid listingId, Stream fileStream, string fileName, string contentType)
    {
        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            // Verify the listing exists and user owns it
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null)
            {
                ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(listingId));
            }

            var currentUserId = _currentUserService.GetUserId();
            if (listing.OwnerId != currentUserId)
            {
                ErrorHelper.ThrowErrorOperation(ListingServiceErrors.NotOwner());
            }

            // Validate file first
            var validation = await _photoStorageService.ValidateFileAsync(fileStream, fileName, contentType);
            if (!validation.IsValid)
            {
                ErrorHelper.ThrowError(ListingServiceErrors.FileValidationFailed(validation.ErrorMessage));
            }

            // Reset stream position after validation
            fileStream.Position = 0;

            // Upload file and get URL
            var photoUrl = await _photoStorageService.UploadPhotoAsync(listingId, fileStream, fileName, contentType);
            
            // Save photo URL to database
            var photo = await _photoRepository.AddPhotoAsync(listingId, photoUrl);
            return photo.Id;
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }

        return result.Value;
    }

    public async Task RemovePhotoAsync(Guid listingId, Guid photoId)
    {
        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            // Verify the listing exists and user owns it
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null)
            {
                ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(listingId));
            }

            var currentUserId = _currentUserService.GetUserId();
            if (listing.OwnerId != currentUserId)
            {
                ErrorHelper.ThrowErrorOperation(ListingServiceErrors.NotOwner());
            }

            // Get photo to retrieve its URL before deletion
            var photo = await _photoRepository.GetByIdAsync(photoId);
            if (photo == null)
            {
                ErrorHelper.ThrowError(ListingServiceErrors.PhotoNotFound(photoId));
            }

            // Verify photo belongs to this listing
            if (photo.ListingId != listingId)
            {
                ErrorHelper.ThrowError(ListingServiceErrors.PhotoNotBelongsToListing(photoId, listingId));
            }

            // Delete file from storage
            await _photoStorageService.DeletePhotoAsync(photo.Url);

            // Remove photo from database
            await _photoRepository.RemovePhotoAsync(listingId, photoId);
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }
    }

    public async Task ReorderPhotosAsync(Guid listingId, List<Guid> orderedPhotoIds)
    {
        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            // Verify the listing exists and user owns it
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null)
            {
                ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(listingId));
            }

            var currentUserId = _currentUserService.GetUserId();
            if (listing.OwnerId != currentUserId)
            {
                ErrorHelper.ThrowErrorOperation(ListingServiceErrors.NotOwner());
            }

            if (orderedPhotoIds == null || !orderedPhotoIds.Any())
            {
                ErrorHelper.ThrowError(ListingServiceErrors.PhotoOrderIdsEmpty());
            }

            // Verify all photos belong to this listing
            var existingPhotos = await _photoRepository.GetPhotosByListingIdAsync(listingId);
            var existingPhotoIds = existingPhotos.Select(p => p.Id).ToHashSet();
            
            var invalidPhotoIds = orderedPhotoIds.Where(id => !existingPhotoIds.Contains(id)).ToList();
            if (invalidPhotoIds.Any())
            {
                ErrorHelper.ThrowError(ListingServiceErrors.PhotosNotBelongToListing(invalidPhotoIds));
            }

            await _photoRepository.ReorderPhotosAsync(listingId, orderedPhotoIds);
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }
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

    public async Task<(Stream Stream, string ContentType, string FileName)?> GetPhotoStreamAsync(string photoUrl)
    {
        return await _photoStorageService.GetPhotoStreamAsync(photoUrl);
    }

    private static PhotoDto MapToDto(ListingPhoto photo) => new PhotoDto(
        photo.Id,
        photo.ListingId,
        photo.Url,
        photo.Order
    );
}
