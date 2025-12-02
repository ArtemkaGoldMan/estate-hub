using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.DTO;

public record ResolveReportInput(
    Guid ReportId,
    string Resolution,
    string? ModeratorNotes = null,
    bool UnpublishListing = false,
    string? UnpublishReason = null
);

public record DismissReportInput(
    Guid ReportId,
    string? ModeratorNotes = null
);
