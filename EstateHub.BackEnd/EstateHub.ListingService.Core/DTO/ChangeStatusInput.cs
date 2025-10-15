using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Core.DTO;

public record ChangeStatusInput(
    ListingStatus NewStatus
);
