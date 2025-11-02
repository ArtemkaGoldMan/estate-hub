using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Models;
using EstateHub.SharedKernel.API.Interfaces;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Requests;
using Microsoft.Extensions.Logging;

namespace EstateHub.ListingService.Core.Mappers;

/// <summary>
/// Helper class for mapping Report domain models to ReportDto.
/// Handles fetching related data (user emails, listing titles) via gRPC and repository.
/// </summary>
public class ReportDtoMapper
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<ReportDtoMapper> _logger;

    public ReportDtoMapper(
        IUserServiceClient userServiceClient,
        IListingRepository listingRepository,
        ILogger<ReportDtoMapper> logger)
    {
        _userServiceClient = userServiceClient;
        _listingRepository = listingRepository;
        _logger = logger;
    }

    /// <summary>
    /// Maps a single report to DTO with related data
    /// </summary>
    public async Task<ReportDto> MapToDtoAsync(Report report)
    {
        var userIdsToFetch = CollectUserIds(report);
        var listingIdsToFetch = new List<Guid> { report.ListingId };

        var userEmails = await FetchUserEmailsAsync(userIdsToFetch);
        var listingTitles = await FetchListingTitlesAsync(listingIdsToFetch);

        return BuildReportDto(report, userEmails, listingTitles);
    }

    /// <summary>
    /// Maps multiple reports to DTOs with batch fetching for performance
    /// </summary>
    public async Task<IEnumerable<ReportDto>> MapToDtosAsync(IEnumerable<Report> reports)
    {
        var reportsList = reports.ToList();
        if (!reportsList.Any())
        {
            return Enumerable.Empty<ReportDto>();
        }

        // Collect unique IDs for batch fetching
        var (userIds, listingIds) = CollectAllIds(reportsList);

        // Batch fetch related data
        var userEmails = await FetchUserEmailsAsync(userIds);
        var listingTitles = await FetchListingTitlesAsync(listingIds);

        // Map all reports using fetched data
        return reportsList.Select(report => BuildReportDto(report, userEmails, listingTitles));
    }

    private static List<Guid> CollectUserIds(Report report)
    {
        var userIds = new List<Guid>();
        
        if (report.ReporterId != Guid.Empty)
            userIds.Add(report.ReporterId);
        
        if (report.ModeratorId.HasValue && report.ModeratorId.Value != Guid.Empty)
            userIds.Add(report.ModeratorId.Value);

        return userIds;
    }

    private static (List<Guid> UserIds, List<Guid> ListingIds) CollectAllIds(IEnumerable<Report> reports)
    {
        var userIds = new HashSet<Guid>();
        var listingIds = new HashSet<Guid>();

        foreach (var report in reports)
        {
            if (report.ReporterId != Guid.Empty)
                userIds.Add(report.ReporterId);
            
            if (report.ModeratorId.HasValue && report.ModeratorId.Value != Guid.Empty)
                userIds.Add(report.ModeratorId.Value);
            
            if (report.ListingId != Guid.Empty)
                listingIds.Add(report.ListingId);
        }

        return (userIds.ToList(), listingIds.ToList());
    }

    private static ReportDto BuildReportDto(
        Report report,
        Dictionary<Guid, string> userEmails,
        Dictionary<Guid, string> listingTitles)
    {
        var reporterEmail = GetEmail(userEmails, report.ReporterId);
        var moderatorEmail = report.ModeratorId.HasValue 
            ? GetEmail(userEmails, report.ModeratorId.Value) 
            : null;
        var listingTitle = GetListingTitle(listingTitles, report.ListingId);

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
            reporterEmail,
            moderatorEmail,
            listingTitle
        );
    }

    private static string? GetEmail(Dictionary<Guid, string> userEmails, Guid userId)
    {
        return userId != Guid.Empty && userEmails.TryGetValue(userId, out var email) 
            ? email 
            : null;
    }

    private static string? GetListingTitle(Dictionary<Guid, string> listingTitles, Guid listingId)
    {
        return listingTitles.TryGetValue(listingId, out var title) 
            ? title 
            : null;
    }

    private async Task<Dictionary<Guid, string>> FetchUserEmailsAsync(List<Guid> userIds)
    {
        if (!userIds.Any())
        {
            return new Dictionary<Guid, string>();
        }

        try
        {
            var request = new GetUsersByIdsRequest { Ids = userIds };
            var response = await _userServiceClient.GetUsersByIdsAsync(request);

            if (response?.Users == null)
            {
                _logger.LogWarning("Failed to fetch user emails via gRPC for {Count} users", userIds.Count);
                return new Dictionary<Guid, string>();
            }

            var userEmails = response.Users
                .Where(u => u != null && !string.IsNullOrEmpty(u.Email))
                .ToDictionary(u => u.Id, u => u.Email);

            _logger.LogDebug("Fetched {Count} user emails via gRPC", userEmails.Count);
            return userEmails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user emails via gRPC for {Count} users", userIds.Count);
            return new Dictionary<Guid, string>();
        }
    }

    private async Task<Dictionary<Guid, string>> FetchListingTitlesAsync(List<Guid> listingIds)
    {
        if (!listingIds.Any())
        {
            return new Dictionary<Guid, string>();
        }

        try
        {
            // Fetch listings in parallel for better performance
            var listingTasks = listingIds.Select(async id =>
            {
                var listing = await _listingRepository.GetByIdAsync(id);
                return listing != null ? (id, listing.Title) : ((Guid?)null, (string?)null);
            });

            var results = await Task.WhenAll(listingTasks);

            var titles = new Dictionary<Guid, string>();
            foreach (var (id, title) in results)
            {
                if (id.HasValue && !string.IsNullOrEmpty(title))
                {
                    titles[id.Value] = title;
                }
            }

            _logger.LogDebug("Fetched {Count} listing titles", titles.Count);
            return titles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching listing titles for {Count} listings", listingIds.Count);
            return new Dictionary<Guid, string>();
        }
    }
}

