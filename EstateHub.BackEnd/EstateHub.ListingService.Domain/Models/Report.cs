using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.Models;

public record Report
{
    public Guid Id { get; init; }
    public Guid ReporterId { get; init; }
    public Guid ListingId { get; init; }
    public ReportReason Reason { get; init; }
    public string Description { get; init; }
    public ReportStatus Status { get; init; }
    public Guid? ModeratorId { get; init; }
    public string? ModeratorNotes { get; init; }
    public string? Resolution { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }

    private Report() { } // For EF Core

    public Report(
        Guid reporterId,
        Guid listingId,
        ReportReason reason,
        string description)
    {
        // Validation
        if (reporterId == Guid.Empty)
            throw new ArgumentException("ReporterId cannot be empty", nameof(reporterId));

        if (listingId == Guid.Empty)
            throw new ArgumentException("ListingId cannot be empty", nameof(listingId));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));

        if (description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

        // Initialize properties
        Id = Guid.NewGuid();
        ReporterId = reporterId;
        ListingId = listingId;
        Reason = reason;
        Description = description;
        Status = ReportStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Report AssignToModerator(Guid moderatorId)
    {
        if (Status != ReportStatus.Pending)
            throw new InvalidOperationException("Only pending reports can be assigned to moderators");

        if (moderatorId == Guid.Empty)
            throw new ArgumentException("ModeratorId cannot be empty", nameof(moderatorId));

        return this with
        {
            ModeratorId = moderatorId,
            Status = ReportStatus.UnderReview,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Report Resolve(string resolution, string? moderatorNotes = null)
    {
        if (Status == ReportStatus.Resolved || Status == ReportStatus.Dismissed || Status == ReportStatus.Closed)
            throw new InvalidOperationException("Cannot resolve a report that is already resolved, dismissed, or closed");

        if (string.IsNullOrWhiteSpace(resolution))
            throw new ArgumentException("Resolution cannot be null or empty", nameof(resolution));

        if (resolution.Length > 1000)
            throw new ArgumentException("Resolution cannot exceed 1000 characters", nameof(resolution));

        if (moderatorNotes != null && moderatorNotes.Length > 500)
            throw new ArgumentException("ModeratorNotes cannot exceed 500 characters", nameof(moderatorNotes));

        return this with
        {
            Resolution = resolution,
            ModeratorNotes = moderatorNotes,
            Status = ReportStatus.Resolved,
            ResolvedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Report Dismiss(string? moderatorNotes = null)
    {
        if (Status == ReportStatus.Resolved || Status == ReportStatus.Dismissed || Status == ReportStatus.Closed)
            throw new InvalidOperationException("Cannot dismiss a report that is already resolved, dismissed, or closed");

        if (moderatorNotes != null && moderatorNotes.Length > 500)
            throw new ArgumentException("ModeratorNotes cannot exceed 500 characters", nameof(moderatorNotes));

        return this with
        {
            ModeratorNotes = moderatorNotes,
            Status = ReportStatus.Dismissed,
            ResolvedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Report Close()
    {
        if (Status != ReportStatus.Resolved && Status != ReportStatus.Dismissed)
            throw new InvalidOperationException("Only resolved or dismissed reports can be closed");

        return this with
        {
            Status = ReportStatus.Closed,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
