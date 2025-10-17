using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.DTO;

public record ResolveReportInput(
    Guid ReportId,
    string Resolution,
    string? ModeratorNotes = null
);

public record DismissReportInput(
    Guid ReportId,
    string? ModeratorNotes = null
);
