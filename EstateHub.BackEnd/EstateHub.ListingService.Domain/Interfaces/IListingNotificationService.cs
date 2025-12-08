namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Service interface for sending listing-related email notifications
/// </summary>
public interface IListingNotificationService
{
    /// <summary>
    /// Sends an email notification when a listing is unpublished by an admin
    /// </summary>
    /// <param name="userEmail">The email address of the listing owner</param>
    /// <param name="listingTitle">The title of the unpublished listing</param>
    /// <param name="listingId">The unique identifier of the listing</param>
    /// <param name="reason">The reason for unpublishing the listing</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    Task SendListingUnpublishedNotificationAsync(
        string userEmail,
        string listingTitle,
        Guid listingId,
        string reason,
        CancellationToken cancellationToken = default);
}

