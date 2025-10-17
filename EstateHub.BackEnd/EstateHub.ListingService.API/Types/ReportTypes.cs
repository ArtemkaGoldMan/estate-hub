using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using HotChocolate;

namespace EstateHub.ListingService.API.Types;

public class ReportType
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public Guid ListingId { get; set; }
    public ReportReason Reason { get; set; }
    public string Description { get; set; } = string.Empty;
    public ReportStatus Status { get; set; }
    public Guid? ModeratorId { get; set; }
    public string? ModeratorNotes { get; set; }
    public string? Resolution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ReporterEmail { get; set; }
    public string? ModeratorEmail { get; set; }
    public string? ListingTitle { get; set; }

    public static ReportType FromDto(ReportDto dto) => new()
    {
        Id = dto.Id,
        ReporterId = dto.ReporterId,
        ListingId = dto.ListingId,
        Reason = dto.Reason,
        Description = dto.Description,
        Status = dto.Status,
        ModeratorId = dto.ModeratorId,
        ModeratorNotes = dto.ModeratorNotes,
        Resolution = dto.Resolution,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt,
        ResolvedAt = dto.ResolvedAt,
        ReporterEmail = dto.ReporterEmail,
        ModeratorEmail = dto.ModeratorEmail,
        ListingTitle = dto.ListingTitle
    };
}

public class PagedReportsType
{
    public List<ReportType> Items { get; set; } = new();
    public int Total { get; set; }

    public static PagedReportsType FromDto(PagedResult<ReportDto> dto) => new()
    {
        Items = dto.Items.Select(ReportType.FromDto).ToList(),
        Total = dto.Total
    };
}
