using EstateHub.ListingService.Domain.DTO;

namespace EstateHub.ListingService.Domain.Interfaces;

public interface IContentModerationService
{
    Task<ModerationResult> ModerateAsync(string title, string description, CancellationToken cancellationToken = default);
}




