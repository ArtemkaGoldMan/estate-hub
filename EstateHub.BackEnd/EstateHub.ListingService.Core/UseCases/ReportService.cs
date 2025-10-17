using EstateHub.ListingService.Core.Abstractions;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;
using EstateHub.ListingService.Domain.Enums;
using FluentValidation;

namespace EstateHub.ListingService.Core.UseCases;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IListingRepository _listingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateReportInput> _createValidator;

    public ReportService(
        IReportRepository reportRepository,
        IListingRepository listingRepository,
        ICurrentUserService currentUserService,
        IValidator<CreateReportInput> createValidator)
    {
        _reportRepository = reportRepository;
        _listingRepository = listingRepository;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
    }

    public async Task<PagedResult<ReportDto>> GetAllAsync(ReportFilter? filter, int page, int pageSize)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var reports = await _reportRepository.GetAllAsync(page, pageSize, filter);
        var total = await _reportRepository.GetTotalCountAsync(filter);

        var dtos = await MapToDtosAsync(reports);
        return new PagedResult<ReportDto>(dtos, total, page, pageSize);
    }

    public async Task<ReportDto?> GetByIdAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null) return null;

        var dto = await MapToDtoAsync(report);
        return dto;
    }

    public async Task<PagedResult<ReportDto>> GetMyReportsAsync(int page, int pageSize)
    {
        var currentUserId = _currentUserService.GetUserId();
        
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var reports = await _reportRepository.GetByReporterIdAsync(currentUserId, page, pageSize);
        var total = await _reportRepository.GetTotalCountAsync(new ReportFilter { ReporterId = currentUserId });

        var dtos = await MapToDtosAsync(reports);
        return new PagedResult<ReportDto>(dtos, total, page, pageSize);
    }

    public async Task<PagedResult<ReportDto>> GetReportsForModerationAsync(int page, int pageSize)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var filter = new ReportFilter { Status = ReportStatus.Pending };
        var reports = await _reportRepository.GetAllAsync(page, pageSize, filter);
        var total = await _reportRepository.GetTotalCountAsync(filter);

        var dtos = await MapToDtosAsync(reports);
        return new PagedResult<ReportDto>(dtos, total, page, pageSize);
    }

    public async Task<IEnumerable<ReportDto>> GetReportsByListingIdAsync(Guid listingId)
    {
        var reports = await _reportRepository.GetByListingIdAsync(listingId);
        return await MapToDtosAsync(reports);
    }

    public async Task<Guid> CreateAsync(CreateReportInput input)
    {
        // Validate input
        var validationResult = await _createValidator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        // Check if listing exists
        var listing = await _listingRepository.GetByIdAsync(input.ListingId);
        if (listing == null)
        {
            throw new ArgumentException("Listing not found");
        }

        var currentUserId = _currentUserService.GetUserId();

        // Check if user already reported this listing
        var existingReports = await _reportRepository.GetByListingIdAsync(input.ListingId);
        if (existingReports.Any(r => r.ReporterId == currentUserId && r.Status != ReportStatus.Dismissed))
        {
            throw new InvalidOperationException("You have already reported this listing");
        }

        var report = new Report(
            currentUserId,
            input.ListingId,
            input.Reason,
            input.Description
        );

        await _reportRepository.AddAsync(report);
        return report.Id;
    }

    public async Task ResolveAsync(ResolveReportInput input)
    {
        var report = await _reportRepository.GetByIdAsync(input.ReportId);
        if (report == null)
        {
            throw new ArgumentException("Report not found");
        }

        var currentUserId = _currentUserService.GetUserId();
        
        // Assign to current user if not already assigned
        if (report.ModeratorId == null)
        {
            report.AssignToModerator(currentUserId);
        }

        report.Resolve(input.Resolution, input.ModeratorNotes);
        await _reportRepository.UpdateAsync(report);
    }

    public async Task DismissAsync(DismissReportInput input)
    {
        var report = await _reportRepository.GetByIdAsync(input.ReportId);
        if (report == null)
        {
            throw new ArgumentException("Report not found");
        }

        var currentUserId = _currentUserService.GetUserId();
        
        // Assign to current user if not already assigned
        if (report.ModeratorId == null)
        {
            report.AssignToModerator(currentUserId);
        }

        report.Dismiss(input.ModeratorNotes);
        await _reportRepository.UpdateAsync(report);
    }

    public async Task AssignToModeratorAsync(Guid reportId, Guid moderatorId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
        {
            throw new ArgumentException("Report not found");
        }

        report.AssignToModerator(moderatorId);
        await _reportRepository.UpdateAsync(report);
    }

    public async Task CloseAsync(Guid reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
        {
            throw new ArgumentException("Report not found");
        }

        report.Close();
        await _reportRepository.UpdateAsync(report);
    }

    public async Task DeleteAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
        {
            throw new ArgumentException("Report not found");
        }

        // Only allow deletion of own reports or by admins
        var currentUserId = _currentUserService.GetUserId();
        if (report.ReporterId != currentUserId)
        {
            // TODO: Check if user is admin
            throw new UnauthorizedAccessException("You can only delete your own reports");
        }

        await _reportRepository.DeleteAsync(id);
    }

    private async Task<ReportDto> MapToDtoAsync(Report report)
    {
        // TODO: Get user emails and listing title from external services
        // For now, return null for these fields
        return new ReportDto(
            report.Id,
            report.ReporterId,
            report.ListingId,
            report.Reason,
            report.Description,
            report.Status,
            report.ModeratorId,
            report.ModeratorNotes,
            report.Resolution,
            report.CreatedAt,
            report.UpdatedAt,
            report.ResolvedAt,
            null, // ReporterEmail - would come from UserService
            null, // ModeratorEmail - would come from UserService
            null  // ListingTitle - would come from ListingService
        );
    }

    private async Task<IEnumerable<ReportDto>> MapToDtosAsync(IEnumerable<Report> reports)
    {
        var dtos = new List<ReportDto>();
        foreach (var report in reports)
        {
            dtos.Add(await MapToDtoAsync(report));
        }
        return dtos;
    }
}
