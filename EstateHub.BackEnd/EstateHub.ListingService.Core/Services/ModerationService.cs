using System;
using System.Threading;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Errors;
using EstateHub.SharedKernel.Helpers;
using Microsoft.Extensions.Logging;

namespace EstateHub.ListingService.Core.Services;

public class ModerationService : IModerationService
{
    private readonly IListingRepository _listingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IContentModerationService _contentModerationService;
    private readonly ILogger<ModerationService> _logger;

    public ModerationService(
        IListingRepository listingRepository,
        ICurrentUserService currentUserService,
        IContentModerationService contentModerationService,
        ILogger<ModerationService> logger)
    {
        _listingRepository = listingRepository;
        _currentUserService = currentUserService;
        _contentModerationService = contentModerationService;
        _logger = logger;
    }

    public async Task<ModerationResult> CheckModerationAsync(Guid listingId)
    {
        var checkStartTime = DateTime.UtcNow;
        Guid? currentUserId = null;
        bool isBackgroundTask = false;
        
        _logger.LogInformation(
            "[MODERATION] ===== CHECK MODERATION STARTED ===== ListingId: {ListingId}, Timestamp: {Timestamp}",
            listingId, checkStartTime);
        
        try
        {
            _logger.LogDebug("[MODERATION] Attempting to get current user ID from context...");
            currentUserId = _currentUserService.GetUserId();
            _logger.LogInformation("[MODERATION] Current user ID retrieved: {UserId}", currentUserId);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Background task - no HTTP context available
            isBackgroundTask = true;
            _logger.LogInformation(
                "[MODERATION] No user context available (background task) - ListingId: {ListingId}, ExceptionType: {ExceptionType}, Message: {Message}",
                listingId, ex.GetType().Name, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "[MODERATION] Unexpected error getting user ID - ListingId: {ListingId}, ExceptionType: {ExceptionType}, Message: {Message}",
                listingId, ex.GetType().Name, ex.Message);
            isBackgroundTask = true; // Assume background task if we can't get user
        }

        _logger.LogInformation(
            "[MODERATION] Moderation check context - ListingId: {ListingId}, UserId: {UserId}, IsBackgroundTask: {IsBackground}, ThreadId: {ThreadId}",
            listingId, currentUserId, isBackgroundTask, Thread.CurrentThread.ManagedThreadId);

        try
        {
            _logger.LogDebug("[MODERATION] Fetching listing from repository... ListingId: {ListingId}", listingId);
            var listingFetchStart = DateTime.UtcNow;
            var listing = await _listingRepository.GetByIdAsync(listingId);
            var listingFetchDuration = DateTime.UtcNow - listingFetchStart;
            _logger.LogDebug("[MODERATION] Listing fetched in {Duration}ms", listingFetchDuration.TotalMilliseconds);

            if (listing == null)
            {
                _logger.LogError(
                    "[MODERATION] ===== LISTING NOT FOUND ===== ListingId: {ListingId}, UserId: {UserId}",
                    listingId, currentUserId);
                ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(listingId));
            }

            _logger.LogInformation(
                "[MODERATION] Listing found - ListingId: {ListingId}, OwnerId: {OwnerId}, Title: '{Title}', Status: {Status}, CurrentModerationStatus: {ModerationStatus}",
                listingId, listing.OwnerId, listing.Title, listing.Status, 
                listing.IsModerationApproved.HasValue 
                    ? (listing.IsModerationApproved.Value ? "Approved" : "Rejected")
                    : "NotChecked");

            // Only check ownership if we have a user context (not a background task)
            // Background tasks are trusted since they're triggered by the system after creation/update
            if (!isBackgroundTask && currentUserId.HasValue && listing.OwnerId != currentUserId.Value)
            {
                _logger.LogError(
                    "[MODERATION] ===== UNAUTHORIZED ACCESS ATTEMPT ===== ListingId: {ListingId}, OwnerId: {OwnerId}, RequestingUserId: {UserId}",
                    listingId, listing.OwnerId, currentUserId);
                ErrorHelper.ThrowErrorOperation(ListingServiceErrors.NotOwner());
            }

            var titlePreview = listing.Title ?? "[EMPTY]";
            var descriptionPreview = string.IsNullOrEmpty(listing.Description) 
                ? "[EMPTY]" 
                : listing.Description.Substring(0, Math.Min(100, listing.Description.Length));
            
            _logger.LogInformation(
                "[MODERATION] Calling content moderation service - ListingId: {ListingId}, Title: '{Title}', Description length: {DescLength}, Description preview: '{DescPreview}'",
                listingId, titlePreview, listing.Description?.Length ?? 0, descriptionPreview);
            
            var moderationCallStart = DateTime.UtcNow;
            var result = await _contentModerationService.ModerateAsync(listing.Title, listing.Description);
            var moderationCallDuration = DateTime.UtcNow - moderationCallStart;
            
            _logger.LogInformation(
                "[MODERATION] Content moderation service responded - ListingId: {ListingId}, Duration: {Duration}ms, Approved: {Approved}, HasReason: {HasReason}, HasSuggestions: {HasSuggestions}",
                listingId, moderationCallDuration.TotalMilliseconds, result.IsApproved, 
                !string.IsNullOrEmpty(result.RejectionReason), result.Suggestions?.Any() ?? false);
            
            _logger.LogInformation(
                "[MODERATION] Moderation result details - ListingId: {ListingId}, Approved: {Approved}, Reason: '{Reason}', SuggestionsCount: {SuggestionsCount}",
                listingId, result.IsApproved, result.RejectionReason ?? "N/A", result.Suggestions?.Count ?? 0);
            
            if (result.Suggestions != null && result.Suggestions.Any())
            {
                _logger.LogInformation(
                    "[MODERATION] Moderation suggestions - ListingId: {ListingId}, Suggestions: [{Suggestions}]",
                    listingId, string.Join("; ", result.Suggestions));
            }
            
            // Save moderation result to listing
            _logger.LogDebug("[MODERATION] Preparing to save moderation result to database... ListingId: {ListingId}", listingId);
            var saveStartTime = DateTime.UtcNow;
            var updatedListing = listing.SetModerationResult(result.IsApproved, result.RejectionReason);
            await _listingRepository.UpdateAsync(updatedListing);
            var saveDuration = DateTime.UtcNow - saveStartTime;
            
            _logger.LogInformation(
                "[MODERATION] ===== MODERATION RESULT SAVED TO DATABASE ===== ListingId: {ListingId}, SaveDuration: {SaveDuration}ms, IsModerationApproved: {Approved}, ModerationCheckedAt: {CheckedAt}, Timestamp: {Timestamp}",
                listingId, saveDuration.TotalMilliseconds, updatedListing.IsModerationApproved, 
                updatedListing.ModerationCheckedAt, DateTime.UtcNow);

            var totalDuration = DateTime.UtcNow - checkStartTime;
            _logger.LogInformation(
                "[MODERATION] ===== CHECK MODERATION COMPLETED ===== ListingId: {ListingId}, TotalDuration: {TotalDuration}ms, Result: {Result}",
                listingId, totalDuration.TotalMilliseconds, result.IsApproved ? "APPROVED" : "REJECTED");

            return result;
        }
        catch (Exception ex)
        {
            var totalDuration = DateTime.UtcNow - checkStartTime;
            _logger.LogError(ex,
                "[MODERATION] ===== CHECK MODERATION FAILED ===== ListingId: {ListingId}, UserId: {UserId}, Duration: {Duration}ms, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}",
                listingId, currentUserId, totalDuration.TotalMilliseconds, ex.GetType().Name, ex.Message, ex.StackTrace);
            throw;
        }
    }
}

