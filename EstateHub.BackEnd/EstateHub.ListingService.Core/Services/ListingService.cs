using System;
using System.Threading;
using EstateHub.ListingService.Core.Services;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Models;
using EstateHub.ListingService.Domain.Errors;
using EstateHub.ListingService.Core.Mappers;
using EstateHub.SharedKernel;
using EstateHub.SharedKernel.API.Authorization;
using EstateHub.SharedKernel.Execution;
using EstateHub.SharedKernel.Helpers;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EstateHub.ListingService.Core.Services;

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
    private readonly ResultExecutor<ListingService> _resultExecutor;
    private readonly BackgroundModerationService _backgroundModerationService;

    public ListingService(
        IListingRepository listingRepository,
        ILikedListingRepository likedListingRepository,
        ICurrentUserService currentUserService,
        IValidator<CreateListingInput> createValidator,
        IValidator<UpdateListingInput> updateValidator,
        IValidator<ChangeStatusInput> statusValidator,
        ListingDtoMapper dtoMapper,
        ILogger<ListingService> logger,
        IUnitOfWork unitOfWork,
        BackgroundModerationService backgroundModerationService)
    {
        _listingRepository = listingRepository;
        _likedListingRepository = likedListingRepository;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
        _dtoMapper = dtoMapper;
        _logger = logger;
        _resultExecutor = new ResultExecutor<ListingService>(logger, unitOfWork);
        _backgroundModerationService = backgroundModerationService;
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

    public async Task<PagedResult<ListingDto>> GetArchivedAsync(int page, int pageSize)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Getting archived listings - User: {UserId}, Page: {Page}, PageSize: {PageSize}", 
            currentUserId, page, pageSize);
        
        try
        {
            pageSize = Math.Min(pageSize, 50);
            page = Math.Max(page, 1);

            var listings = await _listingRepository.GetByOwnerIdAsync(currentUserId);
            var archivedListings = listings.Where(l => l.Status == ListingStatus.Archived && !l.IsDeleted);
            var total = archivedListings.Count();

            var pagedListings = archivedListings
                .OrderByDescending(l => l.ArchivedAt ?? l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = await _dtoMapper.MapToDtosAsync(pagedListings, currentUserId);
            _logger.LogDebug("Retrieved {Count} archived listings (Total: {Total})", dtos.Count(), total);
            return new PagedResult<ListingDto>(dtos, total, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting archived listings - User: {UserId}", currentUserId);
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

            var listings = await _listingRepository.GetWithinBoundsAsync(bounds.LatMin, bounds.LatMax, bounds.LonMin, bounds.LonMax, page, pageSize, filter);
            var total = await _listingRepository.GetWithinBoundsCountAsync(bounds.LatMin, bounds.LatMax, bounds.LonMin, bounds.LonMax, filter);

            var currentUserId = GetCurrentUserIdIfAuthenticated();
            var dtos = await _dtoMapper.MapToDtosAsync(listings, currentUserId);
            
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

            var listings = await _listingRepository.SearchAsync(text, page, pageSize, filter);
            var total = await _listingRepository.SearchCountAsync(text, filter);

            var currentUserId = GetCurrentUserIdIfAuthenticated();
            var dtos = await _dtoMapper.MapToDtosAsync(listings, currentUserId);
            
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

        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            // Validate input
            var validationResult = await _createValidator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed for listing creation - User: {UserId}, Errors: {Errors}", 
                    currentUserId, errorMessage);
                var error = ListingServiceErrors.ValidationFailed(errorMessage).WithUserMessage(errorMessage);
                ErrorHelper.ThrowError(error);
            }

            // Sanitize HTML content to prevent XSS attacks
            var sanitizedTitle = EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.Sanitize(input.Title);
            var sanitizedDescription = EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.SanitizeRichText(input.Description);
            var sanitizedAddressLine = EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.Sanitize(input.AddressLine);
            var sanitizedDistrict = EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.Sanitize(input.District);
            var sanitizedCity = EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.Sanitize(input.City);
            var sanitizedPostalCode = EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.Sanitize(input.PostalCode);

            var listing = new Listing(
                currentUserId,
                input.Category,
                input.PropertyType,
                sanitizedTitle,
                sanitizedDescription,
                sanitizedAddressLine,
                sanitizedDistrict,
                sanitizedCity,
                sanitizedPostalCode,
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
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }

        var listingId = result.Value;

        // Enqueue moderation check with retry logic
        _backgroundModerationService.EnqueueModerationCheck(listingId, "create");

        return listingId;
    }

    public async Task UpdateAsync(Guid id, UpdateListingInput input)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Updating listing - ID: {ListingId}, User: {UserId}", id, currentUserId);

        // Check if title/description changed before transaction (for moderation check)
        var listingBeforeUpdate = await _listingRepository.GetByIdAsync(id);
        if (listingBeforeUpdate == null)
        {
            _logger.LogWarning("Listing not found for update - ID: {ListingId}, User: {UserId}", id, currentUserId);
            ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(id));
        }
        
        var titleChanged = input.Title != null && input.Title != listingBeforeUpdate.Title;
        var descriptionChanged = input.Description != null && input.Description != listingBeforeUpdate.Description;
        var shouldCheckModeration = titleChanged || descriptionChanged;

        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed for listing update - ID: {ListingId}, User: {UserId}, Errors: {Errors}", 
                    id, currentUserId, errorMessage);
                var error = ListingServiceErrors.ValidationFailed(errorMessage).WithUserMessage(errorMessage);
                ErrorHelper.ThrowError(error);
            }

            var listing = await _listingRepository.GetByIdAsync(id);

            if (listing == null)
            {
                _logger.LogWarning("Listing not found for update - ID: {ListingId}, User: {UserId}", id, currentUserId);
                ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(id));
            }

            if (listing.OwnerId != currentUserId)
            {
                _logger.LogWarning("Unauthorized update attempt - Listing: {ListingId}, Owner: {OwnerId}, User: {UserId}", 
                    id, listing.OwnerId, currentUserId);
                ErrorHelper.ThrowErrorOperation(ListingServiceErrors.NotOwner());
            }

            // Sanitize HTML content if provided
            var sanitizedTitle = input.Title != null 
                ? EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.Sanitize(input.Title) 
                : listing.Title;
            var sanitizedDescription = input.Description != null 
                ? EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.SanitizeRichText(input.Description) 
                : listing.Description;
            var sanitizedAddressLine = input.AddressLine != null 
                ? EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.Sanitize(input.AddressLine) 
                : listing.AddressLine;
            var sanitizedDistrict = input.District != null 
                ? EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.Sanitize(input.District) 
                : listing.District;
            var sanitizedCity = input.City != null 
                ? EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.Sanitize(input.City) 
                : listing.City;
            var sanitizedPostalCode = input.PostalCode != null 
                ? EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.Sanitize(input.PostalCode) 
                : listing.PostalCode;

            // Create updated listing using 'with' expression
            var updatedListing = listing
                .UpdateBasicInfo(
                    sanitizedTitle,
                    sanitizedDescription
                )
                .UpdateLocation(
                    sanitizedAddressLine,
                    sanitizedDistrict,
                    sanitizedCity,
                    sanitizedPostalCode,
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

            // If listing was Published and content (title/description) changed, 
            // change status back to Draft so it needs to be re-moderated
            if (shouldCheckModeration && listing.Status == ListingStatus.Published)
            {
                _logger.LogInformation(
                    "Published listing content changed - changing status to Draft for re-moderation - ID: {ListingId}, User: {UserId}",
                    id, currentUserId);
                
                updatedListing = updatedListing with
                {
                    Status = ListingStatus.Draft,
                    PublishedAt = null,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            await _listingRepository.UpdateAsync(updatedListing);
            _logger.LogInformation("Listing updated successfully - ID: {ListingId}, User: {UserId}", id, currentUserId);
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }

        // Auto-check moderation after update if title/description changed
        if (shouldCheckModeration)
        {
            _backgroundModerationService.EnqueueModerationCheck(id, $"update(title:{titleChanged},desc:{descriptionChanged})");
        }
        else
        {
            _logger.LogDebug("Skipping moderation check - ListingId: {ListingId}, TitleChanged: {TitleChanged}, DescriptionChanged: {DescChanged}",
                id, titleChanged, descriptionChanged);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Deleting listing - ID: {ListingId}, User: {UserId}", id, currentUserId);

        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var listing = await _listingRepository.GetByIdAsync(id);

            if (listing == null)
            {
                _logger.LogWarning("Listing not found for deletion - ID: {ListingId}, User: {UserId}", id, currentUserId);
                ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(id));
            }

            if (listing.OwnerId != currentUserId)
            {
                _logger.LogWarning("Unauthorized deletion attempt - Listing: {ListingId}, Owner: {OwnerId}, User: {UserId}", 
                    id, listing.OwnerId, currentUserId);
                ErrorHelper.ThrowErrorOperation(ListingServiceErrors.NotOwner());
            }

            await _listingRepository.DeleteAsync(id);
            _logger.LogInformation("Listing deleted successfully - ID: {ListingId}, User: {UserId}", id, currentUserId);
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }
    }

    public async Task ChangeStatusAsync(Guid id, ListingStatus newStatus)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Changing listing status - ID: {ListingId}, NewStatus: {NewStatus}, User: {UserId}", 
            id, newStatus, currentUserId);

        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var listing = await _listingRepository.GetByIdAsync(id);

            if (listing == null)
            {
                _logger.LogWarning("Listing not found for status change - ID: {ListingId}, User: {UserId}", id, currentUserId);
                ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(id));
            }

            if (listing.OwnerId != currentUserId)
            {
                _logger.LogWarning("Unauthorized status change attempt - Listing: {ListingId}, Owner: {OwnerId}, User: {UserId}", 
                    id, listing.OwnerId, currentUserId);
                ErrorHelper.ThrowErrorOperation(ListingServiceErrors.NotOwner());
            }

            // Validate status transition
            var input = new ChangeStatusInput(newStatus);
            var validationResult = await _statusValidator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Invalid status transition - Listing: {ListingId}, Status: {CurrentStatus} -> {NewStatus}, User: {UserId}, Error: {Error}", 
                    id, listing.Status, newStatus, currentUserId, errorMessage);
                var error = ListingServiceErrors.InvalidStatusTransition().WithUserMessage(errorMessage);
                ErrorHelper.ThrowError(error);
            }

            // Use domain methods for status changes (enforces business rules like moderation check)
            Listing updatedListing;
            if (newStatus == ListingStatus.Published)
            {
                updatedListing = listing.Publish(); // This checks moderation
            }
            else if (newStatus == ListingStatus.Archived)
            {
                updatedListing = listing.Archive();
            }
            else if (newStatus == ListingStatus.Draft)
            {
                // Check if we're unarchiving or unpublishing
                if (listing.Status == ListingStatus.Archived)
                {
                    // Unarchive - use domain method to properly clear ArchivedAt
                    updatedListing = listing.Unarchive();
                }
                else
                {
                    // Unpublish - create new listing with Draft status
                    updatedListing = listing with 
                    { 
                        Status = ListingStatus.Draft,
                        PublishedAt = null,
                        UpdatedAt = DateTime.UtcNow
                    };
                }
            }
            else
            {
                throw new InvalidOperationException($"Unsupported status transition to {newStatus}");
            }

            await _listingRepository.UpdateAsync(updatedListing);
            _logger.LogInformation("Listing status changed successfully - ID: {ListingId}, Status: {NewStatus}, User: {UserId}", 
                id, newStatus, currentUserId);
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
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
        return await _listingRepository.GetTotalCountAsync(filter);
    }
}
