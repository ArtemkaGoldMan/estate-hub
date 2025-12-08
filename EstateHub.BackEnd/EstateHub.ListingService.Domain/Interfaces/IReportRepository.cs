using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Models;

namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Repository interface for report data access operations
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Gets a report by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the report</param>
    /// <returns>The report if found, otherwise null</returns>
    Task<Report?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all reports with pagination and optional filtering
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="filter">Optional filter criteria</param>
    /// <returns>Collection of reports</returns>
    Task<IEnumerable<Report>> GetAllAsync(int page, int pageSize, ReportFilter? filter = null);
    
    /// <summary>
    /// Gets all reports created by a specific user
    /// </summary>
    /// <param name="reporterId">The unique identifier of the reporter</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Collection of reports created by the user</returns>
    Task<IEnumerable<Report>> GetByReporterIdAsync(Guid reporterId, int page, int pageSize);
    
    /// <summary>
    /// Gets all reports assigned to a specific moderator
    /// </summary>
    /// <param name="moderatorId">The unique identifier of the moderator</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Collection of reports assigned to the moderator</returns>
    Task<IEnumerable<Report>> GetByModeratorIdAsync(Guid moderatorId, int page, int pageSize);
    
    /// <summary>
    /// Gets all reports for a specific listing
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing</param>
    /// <returns>Collection of reports for the listing</returns>
    Task<IEnumerable<Report>> GetByListingIdAsync(Guid listingId);
    
    /// <summary>
    /// Gets the total count of reports matching optional filter
    /// </summary>
    /// <param name="filter">Optional filter criteria</param>
    /// <returns>Total count of reports</returns>
    Task<int> GetTotalCountAsync(ReportFilter? filter = null);
    
    /// <summary>
    /// Adds a new report to the repository
    /// </summary>
    /// <param name="report">The report entity to add</param>
    Task AddAsync(Report report);
    
    /// <summary>
    /// Updates an existing report in the repository
    /// </summary>
    /// <param name="report">The report entity with updated data</param>
    Task UpdateAsync(Report report);
    
    /// <summary>
    /// Deletes a report from the repository
    /// </summary>
    /// <param name="id">The unique identifier of the report to delete</param>
    Task DeleteAsync(Guid id);
}
