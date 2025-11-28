using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Models;
using EstateHub.ListingService.Domain.Errors;
using EstateHub.ListingService.Core.Mappers;
using EstateHub.SharedKernel.API.Authorization;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EstateHub.ListingService.Core.UseCases;

public class ListingService : IListingService
{
    private readonly IListingRepository _listingRepository;
    private readonly ILikedListingRepository _likedListingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateListingInput> _createValidator;
    private readonly IValidator<UpdateListingInput> _updateValidator;
    private readonly IValidator<ChangeStatusInput> _statusValidator;
    private readonly ListingDtoMapper _dtoMapper;
    private readonly ILogger<ListingService> _logger;

    public ListingService(
        IListingRepository listingRepository,
        ILikedListingRepository likedListingRepository,
        ICurrentUserService currentUserService,
        IValidator<CreateListingInput> createValidator,
        IValidator<UpdateListingInput> updateValidator,
        IValidator<ChangeStatusInput> statusValidator,
        ListingDtoMapper dtoMapper,
        ILogger<ListingService> logger)
    {
        _listingRepository = listingRepository;
        _likedListingRepository = likedListingRepository;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
        _dtoMapper = dtoMapper;
        _logger = logger;
    }

    public async Task<PagedResult<ListingDto>> GetAllAsync(ListingFilter? filter, int page, int pageSize)
    {
        _logger.LogInformation("Getting all listings - Page: {Page}, PageSize: {PageSize}, Filter: {@Filter}", 
            page, pageSize, filter);

        try
        {
            // Cap page size to prevent abuse
            pageSize = Math.Min(pageSize, 50);
            page = Math.Max(page, 1);

            var listings = await _listingRepository.GetAllAsync(page, pageSize, filter);
            var total = await GetTotalCountAsync(filter);

            var currentUserId = GetCurrentUserIdIfAuthenticated();
            var dtos = await _dtoMapper.MapToDtosAsync(listings, currentUserId);

            _logger.LogDebug("Retrieved {Count} listings (Total: {Total})", dtos.Count(), total);
            return new PagedResult<ListingDto>(dtos, total, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all listings - Page: {Page}, PageSize: {PageSize}", page, pageSize);
            throw;
        }
    }

    public async Task<ListingDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting listing by ID: {ListingId}", id);

        try
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
            {
                _logger.LogWarning("Listing not found: {ListingId}", id);
                return null;
            }

            // Check visibility: only Published listings are globally visible
            // Owners can see their own listings regardless of status
            // Admins can only see Published listings
            var currentUserId = GetCurrentUserIdIfAuthenticated();
            var isOwner = currentUserId != null && listing.OwnerId == currentUserId;
            
            if (listing.Status != ListingStatus.Published && !isOwner)
            {
                _logger.LogDebug("Listing {ListingId} is not visible to user {UserId} (Status: {Status}, IsOwner: {IsOwner})", 
                    id, currentUserId, listing.Status, isOwner);
                return null; // Return null instead of throwing to maintain consistency
            }

            _logger.LogDebug("Retrieved listing {ListingId} successfully", id);
            return await _dtoMapper.MapToDtoAsync(listing, currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting listing by ID: {ListingId}", id);
            throw;
        }
    }

    public async Task<PagedResult<ListingDto>> GetMyAsync(int page, int pageSize)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Getting user listings - User: {UserId}, Page: {Page}, PageSize: {PageSize}", 
            currentUserId, page, pageSize);
        
        try
        {
            pageSize = Math.Min(pageSize, 50);
            page = Math.Max(page, 1);

            var listings = await _listingRepository.GetByOwnerIdAsync(currentUserId);
            var total = listings.Count();

            var pagedListings = listings
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = await _dtoMapper.MapToDtosAsync(pagedListings, currentUserId);
            _logger.LogDebug("Retrieved {Count} user listings (Total: {Total})", dtos.Count(), total);
            return new PagedResult<ListingDto>(dtos, total, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user listings - User: {UserId}", currentUserId);
            throw;
        }
    }

    public async Task<PagedResult<ListingDto>> GetLikedAsync(int page, int pageSize)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Getting liked listings - User: {UserId}, Page: {Page}, PageSize: {PageSize}", 
            currentUserId, page, pageSize);
        
        try
        {
            pageSize = Math.Min(pageSize, 50);
            page = Math.Max(page, 1);

            var listings = await _likedListingRepository.GetLikedByUserAsync(currentUserId);
            var total = listings.Count();

            var pagedListings = listings
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = await _dtoMapper.MapToDtosAsync(pagedListings, currentUserId);
            _logger.LogDebug("Retrieved {Count} liked listings (Total: {Total})", dtos.Count(), total);
            return new PagedResult<ListingDto>(dtos, total, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting liked listings - User: {UserId}", currentUserId);
            throw;
        }
    }

    public async Task<PagedResult<ListingDto>> GetWithinBoundsAsync(BoundsInput bounds, int page, int pageSize, ListingFilter? filter = null)
    {
        _logger.LogInformation("Getting listings within bounds - Bounds: {@Bounds}, Page: {Page}, PageSize: {PageSize}, Filter: {@Filter}", 
            bounds, page, pageSize, filter);
        
        try
        {
            pageSize = Math.Min(pageSize, 50);
            page = Math.Max(page, 1);

            var listings = await _listingRepository.GetWithinBoundsAsync(bounds.LatMin, bounds.LatMax, bounds.LonMin, bounds.LonMax, filter);
            var total = listings.Count();

            var pagedListings = listings
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var currentUserId = GetCurrentUserIdIfAuthenticated();
            var dtos = await _dtoMapper.MapToDtosAsync(pagedListings, currentUserId);
            
            _logger.LogDebug("Retrieved {Count} listings within bounds (Total: {Total})", dtos.Count(), total);
            return new PagedResult<ListingDto>(dtos, total, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting listings within bounds - Bounds: {@Bounds}, Filter: {@Filter}", bounds, filter);
            throw;
        }
    }

    public async Task<PagedResult<ListingDto>> SearchAsync(string text, ListingFilter? filter, int page, int pageSize)
    {
        _logger.LogInformation("Searching listings - Text: {Text}, Filter: {@Filter}, Page: {Page}, PageSize: {PageSize}", 
            text, filter, page, pageSize);
        
        try
        {
            pageSize = Math.Min(pageSize, 50);
            page = Math.Max(page, 1);

            var listings = await _listingRepository.SearchAsync(text, filter);
            var total = listings.Count();

            var pagedListings = listings
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var currentUserId = GetCurrentUserIdIfAuthenticated();
            var dtos = await _dtoMapper.MapToDtosAsync(pagedListings, currentUserId);
            
            _logger.LogDebug("Found {Count} listings matching search (Total: {Total})", dtos.Count(), total);
            return new PagedResult<ListingDto>(dtos, total, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching listings - Text: {Text}", text);
            throw;
        }
    }

    public async Task<Guid> CreateAsync(CreateListingInput input)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Creating listing - User: {UserId}, Title: {Title}", currentUserId, input.Title);

        try
        {
            // Validate input
            var validationResult = await _createValidator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed for listing creation - User: {UserId}, Errors: {Errors}", 
                    currentUserId, errorMessage);
                throw new ArgumentException(errorMessage)
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.ValidationFailed(errorMessage).Code }
                };
            }

            var listing = new Listing(
                currentUserId,
                input.Category,
                input.PropertyType,
                input.Title,
                input.Description,
                input.AddressLine,
                input.District,
                input.City,
                input.PostalCode,
                input.Latitude,
                input.Longitude,
                input.SquareMeters,
                input.Rooms,
                input.Condition,
                input.HasBalcony,
                input.HasElevator,
                input.HasParkingSpace,
                input.HasSecurity,
                input.HasStorageRoom,
                input.Floor,
                input.FloorCount,
                input.BuildYear,
                input.PricePln,
                input.MonthlyRentPln
            );

            await _listingRepository.AddAsync(listing);
            _logger.LogInformation("Listing created successfully - ID: {ListingId}, User: {UserId}", 
                listing.Id, currentUserId);
            return listing.Id;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, "Error creating listing - User: {UserId}", currentUserId);
            throw;
        }
    }

    public async Task UpdateAsync(Guid id, UpdateListingInput input)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Updating listing - ID: {ListingId}, User: {UserId}", id, currentUserId);

        try
        {
            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed for listing update - ID: {ListingId}, User: {UserId}, Errors: {Errors}", 
                    id, currentUserId, errorMessage);
                throw new ArgumentException(errorMessage)
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.ValidationFailed(errorMessage).Code }
                };
            }

            var listing = await _listingRepository.GetByIdAsync(id);

            if (listing == null)
            {
                _logger.LogWarning("Listing not found for update - ID: {ListingId}, User: {UserId}", id, currentUserId);
                throw new KeyNotFoundException($"Listing with ID {id} not found")
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.ListingNotFound(id).Code }
                };
            }

            if (listing.OwnerId != currentUserId)
            {
                _logger.LogWarning("Unauthorized update attempt - Listing: {ListingId}, Owner: {OwnerId}, User: {UserId}", 
                    id, listing.OwnerId, currentUserId);
                throw new InvalidOperationException("Forbidden: You can only update your own listings")
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.NotOwner().Code }
                };
            }

            // Create updated listing using 'with' expression
            var updatedListing = listing
                .UpdateBasicInfo(
                    input.Title ?? listing.Title,
                    input.Description ?? listing.Description
                )
                .UpdateLocation(
                    input.AddressLine ?? listing.AddressLine,
                    input.District ?? listing.District,
                    input.City ?? listing.City,
                    input.PostalCode ?? listing.PostalCode,
                    input.Latitude ?? listing.Latitude,
                    input.Longitude ?? listing.Longitude
                )
                .UpdatePropertyDetails(
                    input.SquareMeters ?? listing.SquareMeters,
                    input.Rooms ?? listing.Rooms,
                    input.Floor ?? listing.Floor,
                    input.FloorCount ?? listing.FloorCount,
                    input.BuildYear ?? listing.BuildYear,
                    input.Condition ?? listing.Condition
                )
                .UpdateAmenities(
                    input.HasBalcony ?? listing.HasBalcony,
                    input.HasElevator ?? listing.HasElevator,
                    input.HasParkingSpace ?? listing.HasParkingSpace,
                    input.HasSecurity ?? listing.HasSecurity,
                    input.HasStorageRoom ?? listing.HasStorageRoom
                )
                .UpdatePricing(
                    input.PricePln ?? listing.PricePln,
                    input.MonthlyRentPln ?? listing.MonthlyRentPln
                );

            await _listingRepository.UpdateAsync(updatedListing);
            _logger.LogInformation("Listing updated successfully - ID: {ListingId}, User: {UserId}", id, currentUserId);
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is KeyNotFoundException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error updating listing - ID: {ListingId}, User: {UserId}", id, currentUserId);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Deleting listing - ID: {ListingId}, User: {UserId}", id, currentUserId);

        try
        {
            var listing = await _listingRepository.GetByIdAsync(id);

            if (listing == null)
            {
                _logger.LogWarning("Listing not found for deletion - ID: {ListingId}, User: {UserId}", id, currentUserId);
                throw new KeyNotFoundException($"Listing with ID {id} not found")
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.ListingNotFound(id).Code }
                };
            }

            if (listing.OwnerId != currentUserId)
            {
                _logger.LogWarning("Unauthorized deletion attempt - Listing: {ListingId}, Owner: {OwnerId}, User: {UserId}", 
                    id, listing.OwnerId, currentUserId);
                throw new InvalidOperationException("Forbidden: You can only delete your own listings")
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.NotOwner().Code }
                };
            }

            await _listingRepository.DeleteAsync(id);
            _logger.LogInformation("Listing deleted successfully - ID: {ListingId}, User: {UserId}", id, currentUserId);
        }
        catch (Exception ex) when (!(ex is KeyNotFoundException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error deleting listing - ID: {ListingId}, User: {UserId}", id, currentUserId);
            throw;
        }
    }

    public async Task ChangeStatusAsync(Guid id, ListingStatus newStatus)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Changing listing status - ID: {ListingId}, NewStatus: {NewStatus}, User: {UserId}", 
            id, newStatus, currentUserId);

        try
        {
            var listing = await _listingRepository.GetByIdAsync(id);

            if (listing == null)
            {
                _logger.LogWarning("Listing not found for status change - ID: {ListingId}, User: {UserId}", id, currentUserId);
                throw new KeyNotFoundException($"Listing with ID {id} not found")
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.ListingNotFound(id).Code }
                };
            }

            if (listing.OwnerId != currentUserId)
            {
                _logger.LogWarning("Unauthorized status change attempt - Listing: {ListingId}, Owner: {OwnerId}, User: {UserId}", 
                    id, listing.OwnerId, currentUserId);
                throw new InvalidOperationException("Forbidden: You can only change status of your own listings")
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.NotOwner().Code }
                };
            }

            // Validate status transition
            var input = new ChangeStatusInput(newStatus);
            var validationResult = await _statusValidator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Invalid status transition - Listing: {ListingId}, Status: {CurrentStatus} -> {NewStatus}, User: {UserId}, Error: {Error}", 
                    id, listing.Status, newStatus, currentUserId, errorMessage);
                throw new ArgumentException(errorMessage)
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.InvalidStatusTransition().Code }
                };
            }

            await _listingRepository.UpdateStatusAsync(id, newStatus);
            _logger.LogInformation("Listing status changed successfully - ID: {ListingId}, Status: {NewStatus}, User: {UserId}", 
                id, newStatus, currentUserId);
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is KeyNotFoundException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error changing listing status - ID: {ListingId}, Status: {NewStatus}, User: {UserId}", 
                id, newStatus, currentUserId);
            throw;
        }
    }

    public async Task LikeAsync(Guid listingId)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Liking listing - ID: {ListingId}, User: {UserId}", listingId, currentUserId);

        try
        {
            await _likedListingRepository.LikeAsync(currentUserId, listingId);
            _logger.LogDebug("Listing liked successfully - ID: {ListingId}, User: {UserId}", listingId, currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking listing - ID: {ListingId}, User: {UserId}", listingId, currentUserId);
            throw;
        }
    }

    public async Task UnlikeAsync(Guid listingId)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Unliking listing - ID: {ListingId}, User: {UserId}", listingId, currentUserId);

        try
        {
            await _likedListingRepository.UnlikeAsync(currentUserId, listingId);
            _logger.LogDebug("Listing unliked successfully - ID: {ListingId}, User: {UserId}", listingId, currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unliking listing - ID: {ListingId}, User: {UserId}", listingId, currentUserId);
            throw;
        }
    }

    private Guid? GetCurrentUserIdIfAuthenticated()
    {
        try
        {
            return _currentUserService.GetUserId();
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private async Task<int> GetTotalCountAsync(ListingFilter? filter)
    {
        var allListings = await _listingRepository.GetAllAsync(1, int.MaxValue, filter);
        return allListings.Count();
    }
}
