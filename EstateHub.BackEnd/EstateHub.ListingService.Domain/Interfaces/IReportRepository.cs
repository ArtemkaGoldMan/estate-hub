using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Models;

namespace EstateHub.ListingService.Domain.Interfaces;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id);
    Task<IEnumerable<Report>> GetAllAsync(int page, int pageSize, ReportFilter? filter = null);
    Task<IEnumerable<Report>> GetByReporterIdAsync(Guid reporterId, int page, int pageSize);
    Task<IEnumerable<Report>> GetByModeratorIdAsync(Guid moderatorId, int page, int pageSize);
    Task<IEnumerable<Report>> GetByListingIdAsync(Guid listingId);
    Task<int> GetTotalCountAsync(ReportFilter? filter = null);
    Task AddAsync(Report report);
    Task UpdateAsync(Report report);
    Task DeleteAsync(Guid id);
}
