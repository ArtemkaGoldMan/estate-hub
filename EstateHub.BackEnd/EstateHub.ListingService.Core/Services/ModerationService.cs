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
        _logger.LogInformation("Checking moderation for listing - ID: {ListingId}", listingId);

        try
        {
            var listing = await _listingRepository.GetByIdAsync(listingId);

            if (listing == null)
            {
                _logger.LogWarning("Listing not found for moderation check - ID: {ListingId}", listingId);
                ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(listingId));
            }

            // Check authorization only if user context is available (not in background tasks)
            try
            {
                var currentUserId = _currentUserService.GetUserId();
                if (listing.OwnerId != currentUserId)
                {
                    _logger.LogWarning("Unauthorized moderation check attempt - Listing: {ListingId}, Owner: {OwnerId}, User: {UserId}", 
                        listingId, listing.OwnerId, currentUserId);
                    ErrorHelper.ThrowErrorOperation(ListingServiceErrors.NotOwner());
                }
                _logger.LogDebug("Authorization check passed - Listing: {ListingId}, User: {UserId}", listingId, currentUserId);
            }
            catch (UnauthorizedAccessException)
            {
                // No user context available (background task) - skip authorization check
                // This is safe because background moderation only happens for listings just created/updated
                _logger.LogDebug("No user context available (background task) - skipping authorization check for Listing: {ListingId}", listingId);
            }

            var result = await _contentModerationService.ModerateAsync(listing.Title, listing.Description);
            
            // Save moderation result to the listing
            // Note: UpdateAsync already calls SaveChangesAsync internally
            var updatedListing = listing.SetModerationResult(result.IsApproved, result.RejectionReason);
            await _listingRepository.UpdateAsync(updatedListing);
            
            _logger.LogInformation(
                "Moderation check completed and saved - Listing: {ListingId}, Approved: {Approved}, Reason: {Reason}",
                listingId,
                result.IsApproved,
                result.RejectionReason ?? "N/A");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking moderation - Listing: {ListingId}", listingId);
            throw;
        }
    }
}

