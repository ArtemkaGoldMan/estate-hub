namespace EstateHub.ListingService.Domain.Interfaces;

public interface ILocationAIService
{
    Task<string> AskAboutLocationAsync(
        string question,
        string city,
        string? district,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken = default);
}

