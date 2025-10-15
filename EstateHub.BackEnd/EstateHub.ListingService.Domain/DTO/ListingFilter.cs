using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.DTO;

public record ListingFilter(
    string? City = null,
    string? District = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    decimal? MinMeters = null,
    decimal? MaxMeters = null,
    int? MinRooms = null,
    int? MaxRooms = null,
    bool? HasElevator = null,
    bool? HasParkingSpace = null,
    ListingCategory? Category = null,
    PropertyType? PropertyType = null,
    Condition? Condition = null,
    bool? HasBalcony = null,
    bool? HasSecurity = null,
    bool? HasStorageRoom = null
);
