using EstateHub.ListingService.Core.Abstractions;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.SharedKernel.API.Authorization.Attributes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types;

public class ReportQueries
{
    [Authorize]
    [RequirePermission("ViewReports")] // Only Moderators and Admins
    public async Task<ReportType?> GetReport(
        Guid id,
        [Service] IReportService reportService)
    {
        var result = await reportService.GetByIdAsync(id);
        return result != null ? ReportType.FromDto(result) : null;
    }

    [Authorize]
    [RequirePermission("ViewReports")] // Only Moderators and Admins
    public async Task<PagedReportsType> GetReports(
        ReportFilterType? filter,
        int page,
        int pageSize,
        [Service] IReportService reportService)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);
        
        var filterDto = filter?.ToDto();
        var result = await reportService.GetAllAsync(filterDto, page, pageSize);
        return PagedReportsType.FromDto(result);
    }

    [Authorize]
    public async Task<PagedReportsType> GetMyReports( // Users can only see their own reports
        int page,
        int pageSize,
        [Service] IReportService reportService)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);
        
        var result = await reportService.GetMyReportsAsync(page, pageSize);
        return PagedReportsType.FromDto(result);
    }

    [Authorize]
    [RequirePermission("ManageReports")]
    public async Task<PagedReportsType> GetReportsForModeration(
        int page,
        int pageSize,
        [Service] IReportService reportService)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);
        
        var result = await reportService.GetReportsForModerationAsync(page, pageSize);
        return PagedReportsType.FromDto(result);
    }

    [Authorize]
    [RequirePermission("ViewReports")] // Only Moderators and Admins
    public async Task<List<ReportType>> GetReportsByListing(
        Guid listingId,
        [Service] IReportService reportService)
    {
        var result = await reportService.GetReportsByListingIdAsync(listingId);
        return result.Select(ReportType.FromDto).ToList();
    }
}
