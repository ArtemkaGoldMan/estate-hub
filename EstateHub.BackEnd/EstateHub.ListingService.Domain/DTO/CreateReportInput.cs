using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.DTO;

public record CreateReportInput(
    Guid ListingId,
    ReportReason Reason,
    string Description
);
