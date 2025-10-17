using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.DTO;

public record ReportDto(
    Guid Id,
    Guid ReporterId,
    Guid ListingId,
    ReportReason Reason,
    string Description,
    ReportStatus Status,
    Guid? ModeratorId,
    string? ModeratorNotes,
    string? Resolution,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ResolvedAt,
    string? ReporterEmail, // For display purposes
    string? ModeratorEmail, // For display purposes
    string? ListingTitle // For display purposes
);
