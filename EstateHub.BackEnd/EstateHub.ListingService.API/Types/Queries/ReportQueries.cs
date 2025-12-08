using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.API.Types.OutputTypes;
using EstateHub.ListingService.API.Types.InputTypes;
using EstateHub.SharedKernel.API.Authorization.Attributes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types.Queries;

[ExtendObjectType(typeof(Queries))]
public class ReportQueries
{
    [Authorize]
    [RequirePermission("ViewReports")]
    public async Task<ReportType?> GetReport(
        Guid id,
        [Service] IReportService reportService)
    {
        var result = await reportService.GetByIdAsync(id);
        return result != null ? ReportType.FromDto(result) : null;
    }

    [Authorize]
    [RequirePermission("ViewReports")]
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
    public async Task<PagedReportsType> GetMyReports(
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
    [RequirePermission("ViewReports")]
    public async Task<List<ReportType>> GetReportsByListing(
        Guid listingId,
        [Service] IReportService reportService)
    {
        var result = await reportService.GetReportsByListingIdAsync(listingId);
        return result.Select(ReportType.FromDto).ToList();
    }
}
