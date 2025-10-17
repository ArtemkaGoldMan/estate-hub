using EstateHub.ListingService.Domain.DTO;

namespace EstateHub.ListingService.Core.Abstractions;

public interface IReportService
{
    // Queries
    Task<PagedResult<ReportDto>> GetAllAsync(ReportFilter? filter, int page, int pageSize);
    Task<ReportDto?> GetByIdAsync(Guid id);
    Task<PagedResult<ReportDto>> GetMyReportsAsync(int page, int pageSize);
    Task<PagedResult<ReportDto>> GetReportsForModerationAsync(int page, int pageSize);
    Task<IEnumerable<ReportDto>> GetReportsByListingIdAsync(Guid listingId);
    
    // Commands
    Task<Guid> CreateAsync(CreateReportInput input);
    Task ResolveAsync(ResolveReportInput input);
    Task DismissAsync(DismissReportInput input);
    Task AssignToModeratorAsync(Guid reportId, Guid moderatorId);
    Task CloseAsync(Guid reportId);
    Task DeleteAsync(Guid id);
}
