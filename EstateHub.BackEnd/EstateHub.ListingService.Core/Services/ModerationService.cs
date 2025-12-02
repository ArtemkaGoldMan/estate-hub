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
        var currentUserId = _currentUserService.GetUserId();
        _logger.LogInformation("Checking moderation for listing - ID: {ListingId}, User: {UserId}", listingId, currentUserId);

        try
        {
            var listing = await _listingRepository.GetByIdAsync(listingId);

            if (listing == null)
            {
                _logger.LogWarning("Listing not found for moderation check - ID: {ListingId}, User: {UserId}", listingId, currentUserId);
                ErrorHelper.ThrowError(ListingServiceErrors.ListingNotFound(listingId));
            }

            if (listing.OwnerId != currentUserId)
            {
                _logger.LogWarning("Unauthorized moderation check attempt - Listing: {ListingId}, Owner: {OwnerId}, User: {UserId}", 
                    listingId, listing.OwnerId, currentUserId);
                ErrorHelper.ThrowErrorOperation(ListingServiceErrors.NotOwner());
            }

            var result = await _contentModerationService.ModerateAsync(listing.Title, listing.Description);
            
            _logger.LogInformation(
                "Moderation check completed - Listing: {ListingId}, Approved: {Approved}",
                listingId,
                result.IsApproved);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking moderation - Listing: {ListingId}, User: {UserId}", listingId, currentUserId);
            throw;
        }
    }
}

