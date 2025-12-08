using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Models;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Errors;
using EstateHub.ListingService.Core.Mappers;
using EstateHub.SharedKernel;
using EstateHub.SharedKernel.API.Authorization;
using EstateHub.SharedKernel.API.Interfaces;
using EstateHub.SharedKernel.Execution;
using EstateHub.SharedKernel.Helpers;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EstateHub.ListingService.Core.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IListingRepository _listingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateReportInput> _createValidator;
    private readonly ReportDtoMapper _dtoMapper;
    private readonly ILogger<ReportService> _logger;
    private readonly ResultExecutor<ReportService> _resultExecutor;
    private readonly IListingNotificationService _notificationService;
    private readonly IUserServiceClient _userServiceClient;

    public ReportService(
        IReportRepository reportRepository,
        IListingRepository listingRepository,
        ICurrentUserService currentUserService,
        IValidator<CreateReportInput> createValidator,
        ReportDtoMapper dtoMapper,
        ILogger<ReportService> logger,
        IUnitOfWork unitOfWork,
        IListingNotificationService notificationService,
        IUserServiceClient userServiceClient)
    {
        _reportRepository = reportRepository;
        _listingRepository = listingRepository;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
        _dtoMapper = dtoMapper;
        _logger = logger;
        _resultExecutor = new ResultExecutor<ReportService>(logger, unitOfWork);
        _notificationService = notificationService;
        _userServiceClient = userServiceClient;
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

        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var validationResult = await _createValidator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed for report creation - User: {UserId}, Listing: {ListingId}, Errors: {Errors}", 
                    currentUserId, input.ListingId, errorMessage);
                var error = ListingServiceErrors.ValidationFailed(errorMessage).WithUserMessage(errorMessage);
                ErrorHelper.ThrowError(error);
            }

            var listing = await _listingRepository.GetByIdAsync(input.ListingId);
            if (listing == null)
            {
                _logger.LogWarning("Listing not found for report - Listing: {ListingId}, User: {UserId}", 
                    input.ListingId, currentUserId);
                ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(input.ListingId));
            }

            var sanitizedDescription = EstateHub.SharedKernel.Helpers.HtmlSanitizerHelper.Sanitize(input.Description);

            var existingReports = await _reportRepository.GetByListingIdAsync(input.ListingId);
            if (existingReports.Any(r => r.ReporterId == currentUserId && r.Status != ReportStatus.Dismissed))
            {
                _logger.LogWarning("User already reported this listing - Listing: {ListingId}, User: {UserId}", 
                    input.ListingId, currentUserId);
                ErrorHelper.ThrowErrorOperation(ListingServiceErrors.AlreadyReported());
            }

            var report = new Report(
                currentUserId,
                input.ListingId,
                input.Reason,
                sanitizedDescription
            );

            await _reportRepository.AddAsync(report);
            _logger.LogInformation("Report created successfully - ID: {ReportId}, User: {UserId}, Listing: {ListingId}", 
                report.Id, currentUserId, input.ListingId);
            return report.Id;
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }

        return result.Value;
    }

    public async Task ResolveAsync(ResolveReportInput input)
    {
        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var report = await _reportRepository.GetByIdAsync(input.ReportId);
            if (report == null)
            {
                ErrorHelper.ThrowError(ListingServiceErrors.ReportNotFound(input.ReportId));
            }

            var currentUserId = _currentUserService.GetUserId();
            
            var reportToUpdate = report.ModeratorId == null
                ? report.AssignToModerator(currentUserId)
                : report;

            var resolvedReport = reportToUpdate.Resolve(input.Resolution, input.ModeratorNotes);
            await _reportRepository.UpdateAsync(resolvedReport);

            if (input.UnpublishListing)
            {
                if (string.IsNullOrWhiteSpace(input.UnpublishReason))
                {
                    ErrorHelper.ThrowErrorOperation(ListingServiceErrors.InvalidInput("UnpublishReason is required when unpublishing a listing"));
                }

                var listing = await _listingRepository.GetByIdAsync(report.ListingId);
                if (listing == null)
                {
                    ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(report.ListingId));
                }

                if (listing.Status == ListingStatus.Published)
                {
                    var unpublishedListing = listing.UnpublishWithReason(input.UnpublishReason!);
                    await _listingRepository.UpdateAsync(unpublishedListing);
                    _logger.LogInformation("Listing unpublished due to report resolution - ListingId: {ListingId}, ReportId: {ReportId}", 
                        listing.Id, report.Id);

                    // Send email notification to listing owner
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var user = await _userServiceClient.GetUserByIdAsync(listing.OwnerId);
                            if (user != null && !string.IsNullOrEmpty(user.Email))
                            {
                                await _notificationService.SendListingUnpublishedNotificationAsync(
                                    user.Email,
                                    listing.Title,
                                    listing.Id,
                                    input.UnpublishReason!);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, 
                                "Failed to send unpublish notification email - ListingId: {ListingId}, OwnerId: {OwnerId}, ReportId: {ReportId}", 
                                listing.Id, listing.OwnerId, report.Id);
                        }
                    });
                }
            }
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }
    }

    public async Task DismissAsync(DismissReportInput input)
    {
        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var report = await _reportRepository.GetByIdAsync(input.ReportId);
            if (report == null)
            {
                ErrorHelper.ThrowError(ListingServiceErrors.ReportNotFound(input.ReportId));
            }

        var currentUserId = _currentUserService.GetUserId();
        
        var reportToUpdate = report.ModeratorId == null
            ? report.AssignToModerator(currentUserId)
            : report;

            var dismissedReport = reportToUpdate.Dismiss(input.ModeratorNotes);
            await _reportRepository.UpdateAsync(dismissedReport);
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }
    }

    public async Task AssignToModeratorAsync(Guid reportId, Guid moderatorId)
    {
        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report == null)
            {
                ErrorHelper.ThrowError(ListingServiceErrors.ReportNotFound(reportId));
            }

            var assignedReport = report.AssignToModerator(moderatorId);
            await _reportRepository.UpdateAsync(assignedReport);
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }
    }

    public async Task CloseAsync(Guid reportId)
    {
        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report == null)
            {
                ErrorHelper.ThrowError(ListingServiceErrors.ReportNotFound(reportId));
            }

            var closedReport = report.Close();
            await _reportRepository.UpdateAsync(closedReport);
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Deleting report - ID: {ReportId}, User: {UserId}", id, currentUserId);

        var result = await _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var report = await _reportRepository.GetByIdAsync(id);
            if (report == null)
            {
                _logger.LogWarning("Report not found for deletion - ID: {ReportId}, User: {UserId}", id, currentUserId);
                ErrorHelper.ThrowError(ListingServiceErrors.ReportNotFound(id));
            }

            if (report.ReporterId != currentUserId)
            {
                if (!_currentUserService.HasPermission(PermissionDefinitions.ManageReports))
                {
                    _logger.LogWarning("Unauthorized deletion attempt - Report: {ReportId}, Reporter: {ReporterId}, User: {UserId}", 
                        id, report.ReporterId, currentUserId);
                    ErrorHelper.ThrowErrorOperation(ListingServiceErrors.UnauthorizedAccess());
                }
            }

            await _reportRepository.DeleteAsync(id);
            _logger.LogInformation("Report deleted successfully - ID: {ReportId}, User: {UserId}", id, currentUserId);
        });

        if (result.IsFailure)
        {
            var error = result.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }
    }
}
