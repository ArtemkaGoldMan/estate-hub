using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.Models;

public class Report
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public Guid ListingId { get; set; }
    public ReportReason Reason { get; set; }
    public string Description { get; set; }
    public ReportStatus Status { get; set; }
    public Guid? ModeratorId { get; set; }
    public string? ModeratorNotes { get; set; }
    public string? Resolution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    private Report() { } // For EF Core

    public Report(
        Guid reporterId,
        Guid listingId,
        ReportReason reason,
        string description)
    {
        Id = Guid.NewGuid();
        ReporterId = reporterId;
        ListingId = listingId;
        Reason = reason;
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Status = ReportStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignToModerator(Guid moderatorId)
    {
        if (Status != ReportStatus.Pending)
            throw new InvalidOperationException("Only pending reports can be assigned to moderators");

        ModeratorId = moderatorId;
        Status = ReportStatus.UnderReview;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resolve(string resolution, string? moderatorNotes = null)
    {
        if (Status == ReportStatus.Resolved || Status == ReportStatus.Dismissed || Status == ReportStatus.Closed)
            throw new InvalidOperationException("Cannot resolve a report that is already resolved, dismissed, or closed");

        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        ModeratorNotes = moderatorNotes;
        Status = ReportStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Dismiss(string? moderatorNotes = null)
    {
        if (Status == ReportStatus.Resolved || Status == ReportStatus.Dismissed || Status == ReportStatus.Closed)
            throw new InvalidOperationException("Cannot dismiss a report that is already resolved, dismissed, or closed");

        ModeratorNotes = moderatorNotes;
        Status = ReportStatus.Dismissed;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status != ReportStatus.Resolved && Status != ReportStatus.Dismissed)
            throw new InvalidOperationException("Only resolved or dismissed reports can be closed");

        Status = ReportStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }
}
