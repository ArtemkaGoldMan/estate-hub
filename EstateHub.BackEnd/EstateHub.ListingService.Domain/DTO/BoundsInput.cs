namespace EstateHub.ListingService.Domain.DTO;

public record BoundsInput(
    decimal LatMin,
    decimal LatMax,
    decimal LonMin,
    decimal LonMax
);
