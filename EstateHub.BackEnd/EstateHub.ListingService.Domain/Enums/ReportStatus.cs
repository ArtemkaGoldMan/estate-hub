namespace EstateHub.ListingService.Domain.Enums;

public enum ReportStatus
{
    Pending,    // Newly created report, waiting for moderator review
    UnderReview, // Moderator is reviewing the report
    Resolved,   // Report has been resolved (action taken)
    Dismissed,  // Report was dismissed (no action needed)
    Closed      // Report is closed (final state)
}
