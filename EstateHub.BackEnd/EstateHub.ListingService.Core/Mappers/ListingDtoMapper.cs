using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Models;

namespace EstateHub.ListingService.Core.Mappers;

/// <summary>
/// Helper class for mapping Listing domain models to ListingDto.
/// Handles checking if listings are liked by the current user with batch optimization.
/// </summary>
public class ListingDtoMapper
{
    private readonly ILikedListingRepository _likedListingRepository;

    public ListingDtoMapper(ILikedListingRepository likedListingRepository)
    {
        _likedListingRepository = likedListingRepository;
    }

    /// <summary>
    /// Maps a single listing to DTO
    /// </summary>
    public async Task<ListingDto> MapToDtoAsync(Listing listing, Guid? currentUserId)
    {
        if (!currentUserId.HasValue)
        {
            return MapToDto(listing, false);
        }

        var likedListingIds = await GetLikedListingIdsAsync(currentUserId);
        var isLiked = likedListingIds.Contains(listing.Id);
        return MapToDto(listing, isLiked);
    }

    /// <summary>
    /// Maps multiple listings to DTOs with batch optimization for liked status
    /// </summary>
    public async Task<List<ListingDto>> MapToDtosAsync(IEnumerable<Listing> listings, Guid? currentUserId)
    {
        var listingsList = listings.ToList();
        if (!listingsList.Any())
        {
            return new List<ListingDto>();
        }
        var likedListingIds = await GetLikedListingIdsAsync(currentUserId);
        return listingsList.Select(listing =>
        {
            var isLiked = currentUserId.HasValue && likedListingIds.Contains(listing.Id);
            return MapToDto(listing, isLiked);
        }).ToList();
    }

    private static ListingDto MapToDto(Listing listing, bool isLikedByCurrentUser)
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
            listing.AddressLine,
            listing.City,
            listing.District,
            listing.PostalCode,
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
            isLikedByCurrentUser,
            listing.IsModerationApproved,
            listing.ModerationCheckedAt,
            listing.ModerationRejectionReason,
            listing.AdminUnpublishReason
        );
    }

    private async Task<HashSet<Guid>> GetLikedListingIdsAsync(Guid? userId)
    {
        if (!userId.HasValue)
        {
            return new HashSet<Guid>();
        }

        var likedListings = await _likedListingRepository.GetLikedByUserAsync(userId.Value);
        return likedListings.Select(l => l.Id).ToHashSet();
    }
}

