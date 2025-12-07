namespace EstateHub.ListingService.Domain.DTO;

public record ModerationResult(
    bool IsApproved,
    string? RejectionReason,
    List<string>? Suggestions
);








