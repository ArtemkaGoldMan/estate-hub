namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Service interface for AI-powered location information queries
/// </summary>
public interface ILocationAIService
{
    /// <summary>
    /// Asks an AI service about a specific location
    /// </summary>
    /// <param name="question">The question to ask about the location</param>
    /// <param name="city">The city name</param>
    /// <param name="district">Optional district or neighborhood name</param>
    /// <param name="latitude">Latitude coordinate of the location</param>
    /// <param name="longitude">Longitude coordinate of the location</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>AI-generated answer about the location</returns>
    Task<string> AskAboutLocationAsync(
        string question,
        string city,
        string? district,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken = default);
}


