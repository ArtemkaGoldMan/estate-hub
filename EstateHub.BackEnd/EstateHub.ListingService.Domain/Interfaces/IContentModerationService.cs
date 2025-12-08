using EstateHub.ListingService.Domain.DTO;

namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Service interface for content moderation using AI
/// </summary>
public interface IContentModerationService
{
    /// <summary>
    /// Moderates listing content for inappropriate material
    /// </summary>
    /// <param name="title">The title of the listing</param>
    /// <param name="description">The description of the listing</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Moderation result indicating approval status and any suggestions</returns>
    Task<ModerationResult> ModerateAsync(string title, string description, CancellationToken cancellationToken = default);
}









