using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Models;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Errors;
using EstateHub.ListingService.Core.Mappers;
using EstateHub.SharedKernel.API.Authorization;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EstateHub.ListingService.Core.UseCases;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IListingRepository _listingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateReportInput> _createValidator;
    private readonly ReportDtoMapper _dtoMapper;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IReportRepository reportRepository,
        IListingRepository listingRepository,
        ICurrentUserService currentUserService,
        IValidator<CreateReportInput> createValidator,
        ReportDtoMapper dtoMapper,
        ILogger<ReportService> logger)
    {
        _reportRepository = reportRepository;
        _listingRepository = listingRepository;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
        _dtoMapper = dtoMapper;
        _logger = logger;
    }

    public async Task<PagedResult<ReportDto>> GetAllAsync(ReportFilter? filter, int page, int pageSize)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var reports = await _reportRepository.GetAllAsync(page, pageSize, filter);
        var total = await _reportRepository.GetTotalCountAsync(filter);

        var dtos = await _dtoMapper.MapToDtosAsync(reports);
        return new PagedResult<ReportDto>(dtos, total, page, pageSize);
    }

    public async Task<ReportDto?> GetByIdAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null) return null;

        return await _dtoMapper.MapToDtoAsync(report);
    }

    public async Task<PagedResult<ReportDto>> GetMyReportsAsync(int page, int pageSize)
    {
        var currentUserId = _currentUserService.GetUserId();
        
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var reports = await _reportRepository.GetByReporterIdAsync(currentUserId, page, pageSize);
        var total = await _reportRepository.GetTotalCountAsync(new ReportFilter { ReporterId = currentUserId });

        var dtos = await _dtoMapper.MapToDtosAsync(reports);
        return new PagedResult<ReportDto>(dtos, total, page, pageSize);
    }

    public async Task<PagedResult<ReportDto>> GetReportsForModerationAsync(int page, int pageSize)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var filter = new ReportFilter { Status = ReportStatus.Pending };
        var reports = await _reportRepository.GetAllAsync(page, pageSize, filter);
        var total = await _reportRepository.GetTotalCountAsync(filter);

        var dtos = await _dtoMapper.MapToDtosAsync(reports);
        return new PagedResult<ReportDto>(dtos, total, page, pageSize);
    }

    public async Task<IEnumerable<ReportDto>> GetReportsByListingIdAsync(Guid listingId)
    {
        var reports = await _reportRepository.GetByListingIdAsync(listingId);
        return await _dtoMapper.MapToDtosAsync(reports);
    }

    public async Task<Guid> CreateAsync(CreateReportInput input)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Creating report - User: {UserId}, Listing: {ListingId}, Reason: {Reason}", 
            currentUserId, input.ListingId, input.Reason);

        try
        {
            // Validate input
            var validationResult = await _createValidator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed for report creation - User: {UserId}, Listing: {ListingId}, Errors: {Errors}", 
                    currentUserId, input.ListingId, errorMessage);
                throw new ArgumentException(errorMessage)
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.ValidationFailed(errorMessage).Code }
                };
            }

            // Check if listing exists
            var listing = await _listingRepository.GetByIdAsync(input.ListingId);
            if (listing == null)
            {
                _logger.LogWarning("Listing not found for report - Listing: {ListingId}, User: {UserId}", 
                    input.ListingId, currentUserId);
                throw new ArgumentException("Listing not found")
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.ListingNotFound(input.ListingId).Code }
                };
            }

            // Check if user already reported this listing
            var existingReports = await _reportRepository.GetByListingIdAsync(input.ListingId);
            if (existingReports.Any(r => r.ReporterId == currentUserId && r.Status != ReportStatus.Dismissed))
            {
                _logger.LogWarning("User already reported this listing - Listing: {ListingId}, User: {UserId}", 
                    input.ListingId, currentUserId);
                throw new InvalidOperationException("You have already reported this listing")
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.AlreadyReported().Code }
                };
            }

            var report = new Report(
                currentUserId,
                input.ListingId,
                input.Reason,
                input.Description
            );

            await _reportRepository.AddAsync(report);
            _logger.LogInformation("Report created successfully - ID: {ReportId}, User: {UserId}, Listing: {ListingId}", 
                report.Id, currentUserId, input.ListingId);
            return report.Id;
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error creating report - User: {UserId}, Listing: {ListingId}", 
                currentUserId, input.ListingId);
            throw;
        }
    }

    public async Task ResolveAsync(ResolveReportInput input)
    {
        var report = await _reportRepository.GetByIdAsync(input.ReportId);
        if (report == null)
        {
            throw new ArgumentException("Report not found");
        }

        var currentUserId = _currentUserService.GetUserId();
        
        // Assign to current user if not already assigned, then resolve
        var reportToUpdate = report.ModeratorId == null
            ? report.AssignToModerator(currentUserId)
            : report;

        var resolvedReport = reportToUpdate.Resolve(input.Resolution, input.ModeratorNotes);
        await _reportRepository.UpdateAsync(resolvedReport);
    }

    public async Task DismissAsync(DismissReportInput input)
    {
        var report = await _reportRepository.GetByIdAsync(input.ReportId);
        if (report == null)
        {
            throw new ArgumentException("Report not found");
        }

        var currentUserId = _currentUserService.GetUserId();
        
        // Assign to current user if not already assigned, then dismiss
        var reportToUpdate = report.ModeratorId == null
            ? report.AssignToModerator(currentUserId)
            : report;

        var dismissedReport = reportToUpdate.Dismiss(input.ModeratorNotes);
        await _reportRepository.UpdateAsync(dismissedReport);
    }

    public async Task AssignToModeratorAsync(Guid reportId, Guid moderatorId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
        {
            throw new ArgumentException("Report not found");
        }

        var assignedReport = report.AssignToModerator(moderatorId);
        await _reportRepository.UpdateAsync(assignedReport);
    }

    public async Task CloseAsync(Guid reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
        {
            throw new ArgumentException("Report not found");
        }

        var closedReport = report.Close();
        await _reportRepository.UpdateAsync(closedReport);
    }

    public async Task DeleteAsync(Guid id)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Deleting report - ID: {ReportId}, User: {UserId}", id, currentUserId);

        try
        {
            var report = await _reportRepository.GetByIdAsync(id);
            if (report == null)
            {
                _logger.LogWarning("Report not found for deletion - ID: {ReportId}, User: {UserId}", id, currentUserId);
                throw new ArgumentException("Report not found")
                {
                    Data = { ["ErrorCode"] = ListingServiceErrors.ReportNotFound(id).Code }
                };
            }

            // Only allow deletion of own reports or by admins
            if (report.ReporterId != currentUserId)
            {
                // Check if user has admin permissions
                if (!_currentUserService.HasPermission(PermissionDefinitions.ManageReports))
                {
                    _logger.LogWarning("Unauthorized deletion attempt - Report: {ReportId}, Reporter: {ReporterId}, User: {UserId}", 
                        id, report.ReporterId, currentUserId);
                    throw new UnauthorizedAccessException("You can only delete your own reports or need admin permissions")
                    {
                        Data = { ["ErrorCode"] = ListingServiceErrors.UnauthorizedAccess().Code }
                    };
                }
            }

            await _reportRepository.DeleteAsync(id);
            _logger.LogInformation("Report deleted successfully - ID: {ReportId}, User: {UserId}", id, currentUserId);
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is UnauthorizedAccessException))
        {
            _logger.LogError(ex, "Error deleting report - ID: {ReportId}, User: {UserId}", id, currentUserId);
            throw;
        }
    }
}
