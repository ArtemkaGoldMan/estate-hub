using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.DTO;

public record ChangeStatusInput(
    ListingStatus NewStatus
);
