using EstateHub.ListingService.Core.Abstractions;
using EstateHub.ListingService.API.Types.InputTypes;
using EstateHub.SharedKernel.API.Authorization.Attributes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types.Mutations;

public class ReportMutations
{
    [Authorize]
    [RequirePermission("CreateReports")]
    public async Task<Guid> CreateReport(
        CreateReportInputType input,
        [Service] IReportService reportService)
    {
        var inputDto = input.ToDto();
        return await reportService.CreateAsync(inputDto);
    }

    [Authorize]
    [RequirePermission("ManageReports")]
    public async Task<bool> ResolveReport(
        ResolveReportInputType input,
        [Service] IReportService reportService)
    {
        var inputDto = input.ToDto();
        await reportService.ResolveAsync(inputDto);
        return true;
    }

    [Authorize]
    [RequirePermission("ManageReports")]
    public async Task<bool> DismissReport(
        DismissReportInputType input,
        [Service] IReportService reportService)
    {
        var inputDto = input.ToDto();
        await reportService.DismissAsync(inputDto);
        return true;
    }

    [Authorize]
    [RequirePermission("ManageReports")]
    public async Task<bool> AssignReportToModerator(
        Guid reportId,
        Guid moderatorId,
        [Service] IReportService reportService)
    {
        await reportService.AssignToModeratorAsync(reportId, moderatorId);
        return true;
    }

    [Authorize]
    [RequirePermission("ManageReports")]
    public async Task<bool> CloseReport(
        Guid reportId,
        [Service] IReportService reportService)
    {
        await reportService.CloseAsync(reportId);
        return true;
    }

    [Authorize]
    public async Task<bool> DeleteReport(
        Guid id,
        [Service] IReportService reportService)
    {
        await reportService.DeleteAsync(id);
        return true;
    }
}
