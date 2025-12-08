using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.API.Types.InputTypes;
using EstateHub.SharedKernel.API.Authorization.Attributes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types.Mutations;

/// <summary>
/// GraphQL mutations for report operations.
/// Extends the base Mutations type with report management functionality.
/// Provides methods for creating reports and managing the moderation workflow.
/// </summary>
[ExtendObjectType(typeof(Mutations))]
public class ReportMutations
{
    /// <summary>
    /// Creates a new report for a listing. Requires authentication and CreateReports permission.
    /// Users can report listings that violate guidelines or contain inappropriate content.
    /// </summary>
    /// <param name="input">The report creation input containing listing ID, reason, and description.</param>
    /// <param name="reportService">The report service injected by HotChocolate.</param>
    /// <returns>The unique identifier (Guid) of the newly created report.</returns>
    [Authorize]
    [RequirePermission("CreateReports")]
    public async Task<Guid> CreateReport(
        CreateReportInputType input,
        [Service] IReportService reportService)
    {
        var inputDto = input.ToDto();
        return await reportService.CreateAsync(inputDto);
    }

    /// <summary>
    /// Resolves a report. Requires authentication and ManageReports permission (Admin only).
    /// When resolving, moderators can optionally unpublish the listing with a reason that will be visible to the listing owner.
    /// </summary>
    /// <param name="input">The report resolution input containing report ID, resolution text, moderator notes, and optional unpublish details.</param>
    /// <param name="reportService">The report service injected by HotChocolate.</param>
    /// <returns>True if the report was successfully resolved.</returns>
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

    /// <summary>
    /// Dismisses a report as invalid or unfounded. Requires authentication and ManageReports permission (Admin only).
    /// Dismissed reports are marked as closed without taking action on the listing.
    /// </summary>
    /// <param name="input">The report dismissal input containing report ID and optional moderator notes.</param>
    /// <param name="reportService">The report service injected by HotChocolate.</param>
    /// <returns>True if the report was successfully dismissed.</returns>
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

    /// <summary>
    /// Assigns a report to a specific moderator for review. Requires authentication and ManageReports permission (Admin only).
    /// </summary>
    /// <param name="reportId">The unique identifier of the report to assign.</param>
    /// <param name="moderatorId">The unique identifier of the moderator to assign the report to.</param>
    /// <param name="reportService">The report service injected by HotChocolate.</param>
    /// <returns>True if the report was successfully assigned to the moderator.</returns>
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

    /// <summary>
    /// Closes a report without resolution or dismissal. Requires authentication and ManageReports permission (Admin only).
    /// Used for reports that are no longer relevant or have been handled through other means.
    /// </summary>
    /// <param name="reportId">The unique identifier of the report to close.</param>
    /// <param name="reportService">The report service injected by HotChocolate.</param>
    /// <returns>True if the report was successfully closed.</returns>
    [Authorize]
    [RequirePermission("ManageReports")]
    public async Task<bool> CloseReport(
        Guid reportId,
        [Service] IReportService reportService)
    {
        await reportService.CloseAsync(reportId);
        return true;
    }

    /// <summary>
    /// Deletes a report. Requires authentication.
    /// Users can only delete their own reports.
    /// </summary>
    /// <param name="id">The unique identifier of the report to delete.</param>
    /// <param name="reportService">The report service injected by HotChocolate.</param>
    /// <returns>True if the report was successfully deleted.</returns>
    [Authorize]
    public async Task<bool> DeleteReport(
        Guid id,
        [Service] IReportService reportService)
    {
        await reportService.DeleteAsync(id);
        return true;
    }
}
