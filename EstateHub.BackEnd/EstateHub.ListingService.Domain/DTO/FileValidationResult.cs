namespace EstateHub.ListingService.Domain.DTO;

public record FileValidationResult(
    bool IsValid,
    string? ErrorMessage = null,
    long FileSize = 0,
    string? DetectedContentType = null
);

