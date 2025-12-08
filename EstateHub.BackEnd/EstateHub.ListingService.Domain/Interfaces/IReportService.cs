using EstateHub.ListingService.Domain.DTO;

namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Service interface for managing listing reports
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Gets all reports with optional filtering and pagination
    /// </summary>
    /// <param name="filter">Optional filter criteria for reports</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paged result containing reports</returns>
    Task<PagedResult<ReportDto>> GetAllAsync(ReportFilter? filter, int page, int pageSize);
    
    /// <summary>
    /// Gets a report by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the report</param>
    /// <returns>The report if found, otherwise null</returns>
    Task<ReportDto?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all reports created by the current user
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paged result containing user's reports</returns>
    Task<PagedResult<ReportDto>> GetMyReportsAsync(int page, int pageSize);
    
    /// <summary>
    /// Gets reports pending moderation
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paged result containing reports for moderation</returns>
    Task<PagedResult<ReportDto>> GetReportsForModerationAsync(int page, int pageSize);
    
    /// <summary>
    /// Gets all reports for a specific listing
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing</param>
    /// <returns>Collection of reports for the listing</returns>
    Task<IEnumerable<ReportDto>> GetReportsByListingIdAsync(Guid listingId);
    
    /// <summary>
    /// Creates a new report
    /// </summary>
    /// <param name="input">Report creation input data</param>
    /// <returns>The unique identifier of the created report</returns>
    Task<Guid> CreateAsync(CreateReportInput input);
    
    /// <summary>
    /// Resolves a report with action taken
    /// </summary>
    /// <param name="input">Report resolution input data</param>
    Task ResolveAsync(ResolveReportInput input);
    
    /// <summary>
    /// Dismisses a report as invalid or unfounded
    /// </summary>
    /// <param name="input">Report dismissal input data</param>
    Task DismissAsync(DismissReportInput input);
    
    /// <summary>
    /// Assigns a report to a moderator
    /// </summary>
    /// <param name="reportId">The unique identifier of the report</param>
    /// <param name="moderatorId">The unique identifier of the moderator</param>
    Task AssignToModeratorAsync(Guid reportId, Guid moderatorId);
    
    /// <summary>
    /// Closes a report
    /// </summary>
    /// <param name="reportId">The unique identifier of the report</param>
    Task CloseAsync(Guid reportId);
    
    /// <summary>
    /// Deletes a report
    /// </summary>
    /// <param name="id">The unique identifier of the report to delete</param>
    Task DeleteAsync(Guid id);
}

