using EstateHub.ListingService.Core.Abstractions;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;
using FluentValidation;

namespace EstateHub.ListingService.Core.UseCases;

public class ListingService : IListingService
{
    private readonly IListingRepository _listingRepository;
    private readonly ILikedListingRepository _likedListingRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateListingInput> _createValidator;
    private readonly IValidator<UpdateListingInput> _updateValidator;
    private readonly IValidator<ChangeStatusInput> _statusValidator;

    public ListingService(
        IListingRepository listingRepository,
        ILikedListingRepository likedListingRepository,
        IPhotoRepository photoRepository,
        ICurrentUserService currentUserService,
        IValidator<CreateListingInput> createValidator,
        IValidator<UpdateListingInput> updateValidator,
        IValidator<ChangeStatusInput> statusValidator)
    {
        _listingRepository = listingRepository;
        _likedListingRepository = likedListingRepository;
        _photoRepository = photoRepository;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
    }

    public async Task<PagedResult<ListingDto>> GetAllAsync(ListingFilter? filter, int page, int pageSize)
    {
        // Cap page size to prevent abuse
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var listings = await _listingRepository.GetAllAsync(page, pageSize, filter);
        var total = await GetTotalCountAsync(filter);

        var currentUserId = GetCurrentUserIdIfAuthenticated();
        var dtos = await MapToDtosAsync(listings, currentUserId);

        return new PagedResult<ListingDto>(dtos, total, page, pageSize);
    }

    public async Task<ListingDto?> GetByIdAsync(Guid id)
    {
        var listing = await _listingRepository.GetByIdAsync(id);
        if (listing == null) return null;

        // Check visibility: only Published listings are globally visible
        var currentUserId = GetCurrentUserIdIfAuthenticated();
        if (listing.Status != ListingStatus.Published && 
            (currentUserId == null || listing.OwnerId != currentUserId))
        {
            return null; // Return null instead of throwing to maintain consistency
        }

        var isLiked = currentUserId.HasValue && await IsLikedByUserAsync(id, currentUserId.Value);
        return MapToDto(listing, isLiked);
    }

    public async Task<PagedResult<ListingDto>> GetMyAsync(int page, int pageSize)
    {
        var currentUserId = _currentUserService.GetUserId();
        
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var listings = await _listingRepository.GetByOwnerIdAsync(currentUserId);
        var total = listings.Count();

        var pagedListings = listings
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = await MapToDtosAsync(pagedListings, currentUserId);
        return new PagedResult<ListingDto>(dtos, total, page, pageSize);
    }

    public async Task<PagedResult<ListingDto>> GetLikedAsync(int page, int pageSize)
    {
        var currentUserId = _currentUserService.GetUserId();
        
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var listings = await _likedListingRepository.GetLikedByUserAsync(currentUserId);
        var total = listings.Count();

        var pagedListings = listings
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = await MapToDtosAsync(pagedListings, currentUserId);
        return new PagedResult<ListingDto>(dtos, total, page, pageSize);
    }

    public async Task<PagedResult<ListingDto>> GetWithinBoundsAsync(BoundsInput bounds, int page, int pageSize)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var listings = await _listingRepository.GetWithinBoundsAsync(bounds.LatMin, bounds.LatMax, bounds.LonMin, bounds.LonMax);
        var total = listings.Count();

        var pagedListings = listings
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var currentUserId = GetCurrentUserIdIfAuthenticated();
        var dtos = await MapToDtosAsync(pagedListings, currentUserId);
        
        return new PagedResult<ListingDto>(dtos, total, page, pageSize);
    }

    public async Task<PagedResult<ListingDto>> SearchAsync(string text, ListingFilter? filter, int page, int pageSize)
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
        var dtos = await MapToDtosAsync(pagedListings, currentUserId);
        
        return new PagedResult<ListingDto>(dtos, total, page, pageSize);
    }

    public async Task<Guid> CreateAsync(CreateListingInput input)
    {
        // Validate input
        var validationResult = await _createValidator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        var currentUserId = _currentUserService.GetUserId();

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
        return listing.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateListingInput input)
    {
        // Validate input
        var validationResult = await _updateValidator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        var currentUserId = _currentUserService.GetUserId();
        var listing = await _listingRepository.GetByIdAsync(id);

        if (listing == null)
            throw new KeyNotFoundException($"Listing with ID {id} not found");

        if (listing.OwnerId != currentUserId)
            throw new InvalidOperationException("Forbidden: You can only update your own listings");

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
    }

    public async Task DeleteAsync(Guid id)
    {
        var currentUserId = _currentUserService.GetUserId();
        var listing = await _listingRepository.GetByIdAsync(id);

        if (listing == null)
            throw new KeyNotFoundException($"Listing with ID {id} not found");

        if (listing.OwnerId != currentUserId)
            throw new InvalidOperationException("Forbidden: You can only delete your own listings");

        await _listingRepository.DeleteAsync(id);
    }

    public async Task ChangeStatusAsync(Guid id, ListingStatus newStatus)
    {
        var currentUserId = _currentUserService.GetUserId();
        var listing = await _listingRepository.GetByIdAsync(id);

        if (listing == null)
            throw new KeyNotFoundException($"Listing with ID {id} not found");

        if (listing.OwnerId != currentUserId)
            throw new InvalidOperationException("Forbidden: You can only change status of your own listings");

        // Validate status transition
        var input = new ChangeStatusInput(newStatus);
        var validationResult = await _statusValidator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        await _listingRepository.UpdateStatusAsync(id, newStatus);
    }

    public async Task LikeAsync(Guid listingId)
    {
        var currentUserId = _currentUserService.GetUserId();
        await _likedListingRepository.LikeAsync(currentUserId, listingId);
    }

    public async Task UnlikeAsync(Guid listingId)
    {
        var currentUserId = _currentUserService.GetUserId();
        await _likedListingRepository.UnlikeAsync(currentUserId, listingId);
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

    private async Task<bool> IsLikedByUserAsync(Guid listingId, Guid userId)
    {
        var likedListings = await _likedListingRepository.GetLikedByUserAsync(userId);
        return likedListings.Any(l => l.Id == listingId);
    }

    private async Task<List<ListingDto>> MapToDtosAsync(IEnumerable<Listing> listings, Guid? currentUserId)
    {
        var result = new List<ListingDto>();
        
        foreach (var listing in listings)
        {
            var isLiked = currentUserId.HasValue && await IsLikedByUserAsync(listing.Id, currentUserId.Value);
            result.Add(MapToDto(listing, isLiked));
        }

        return result;
    }

    private ListingDto MapToDto(Listing listing, bool isLikedByCurrentUser)
    {
        var firstPhotoUrl = listing.Photos?.OrderBy(p => p.Order).FirstOrDefault()?.Url;

        return new ListingDto(
            listing.Id,
            listing.OwnerId,
            listing.Title,
            listing.Description,
            listing.PricePln,
            listing.MonthlyRentPln,
            listing.Status,
            listing.Category,
            listing.PropertyType,
            listing.City,
            listing.District,
            listing.Latitude,
            listing.Longitude,
            listing.SquareMeters,
            listing.Rooms,
            listing.Floor,
            listing.FloorCount,
            listing.BuildYear,
            listing.Condition,
            listing.HasBalcony,
            listing.HasElevator,
            listing.HasParkingSpace,
            listing.HasSecurity,
            listing.HasStorageRoom,
            listing.CreatedAt,
            listing.UpdatedAt,
            listing.PublishedAt,
            listing.ArchivedAt,
            firstPhotoUrl,
            isLikedByCurrentUser
        );
    }

    private async Task<int> GetTotalCountAsync(ListingFilter? filter)
    {
        // This is a simplified implementation - in production you might want to optimize this
        var allListings = await _listingRepository.GetAllAsync(1, int.MaxValue, filter);
        return allListings.Count();
    }
}
