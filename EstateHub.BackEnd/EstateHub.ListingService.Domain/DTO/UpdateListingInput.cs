using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.DTO;

public record UpdateListingInput(
    string? Title,
    string? Description,
    string? AddressLine,
    string? District,
    string? City,
    string? PostalCode,
    decimal? Latitude,
    decimal? Longitude,
    decimal? SquareMeters,
    int? Rooms,
    int? Floor,
    int? FloorCount,
    int? BuildYear,
    Condition? Condition,
    bool? HasBalcony,
    bool? HasElevator,
    bool? HasParkingSpace,
    bool? HasSecurity,
    bool? HasStorageRoom,
    decimal? PricePln,
    decimal? MonthlyRentPln
);
