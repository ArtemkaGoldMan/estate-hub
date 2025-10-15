using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Core.DTO;

public record ListingDto(
    Guid Id,
    Guid OwnerId,
    string Title,
    string Description,
    decimal? PricePln,
    decimal? MonthlyRentPln,
    ListingStatus Status,
    ListingCategory Category,
    PropertyType PropertyType,
    string City,
    string District,
    decimal Latitude,
    decimal Longitude,
    decimal SquareMeters,
    int Rooms,
    int? Floor,
    int? FloorCount,
    int? BuildYear,
    Condition Condition,
    bool HasBalcony,
    bool HasElevator,
    bool HasParkingSpace,
    bool HasSecurity,
    bool HasStorageRoom,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt,
    DateTime? ArchivedAt,
    string? FirstPhotoUrl,
    bool IsLikedByCurrentUser = false
);
