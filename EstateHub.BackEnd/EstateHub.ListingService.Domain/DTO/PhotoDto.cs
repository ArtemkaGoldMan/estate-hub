namespace EstateHub.ListingService.Domain.DTO;

public record PhotoDto(
    Guid Id,
    Guid ListingId,
    string Url,
    int Order
);
