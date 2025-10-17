using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.DTO;

public record ReportFilter(
    ReportStatus? Status = null,
    ReportReason? Reason = null,
    Guid? ReporterId = null,
    Guid? ModeratorId = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null
);
