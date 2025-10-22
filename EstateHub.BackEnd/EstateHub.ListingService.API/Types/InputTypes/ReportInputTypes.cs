using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using HotChocolate;

namespace EstateHub.ListingService.API.Types.InputTypes;

public class CreateReportInputType
{
    public Guid ListingId { get; set; }
    public ReportReason Reason { get; set; }
    public string Description { get; set; } = string.Empty;

    public CreateReportInput ToDto() => new(ListingId, Reason, Description);
}

public class ResolveReportInputType
{
    public Guid ReportId { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public string? ModeratorNotes { get; set; }

    public ResolveReportInput ToDto() => new(ReportId, Resolution, ModeratorNotes);
}

public class DismissReportInputType
{
    public Guid ReportId { get; set; }
    public string? ModeratorNotes { get; set; }

    public DismissReportInput ToDto() => new(ReportId, ModeratorNotes);
}

public class ReportFilterType
{
    public ReportStatus? Status { get; set; }
    public ReportReason? Reason { get; set; }
    public Guid? ReporterId { get; set; }
    public Guid? ModeratorId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public ReportFilter ToDto() => new(Status, Reason, ReporterId, ModeratorId, CreatedFrom, CreatedTo);
}
