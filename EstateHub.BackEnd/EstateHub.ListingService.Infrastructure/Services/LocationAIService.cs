using EstateHub.ListingService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EstateHub.ListingService.Infrastructure.Services;

public class LocationAIService : ILocationAIService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocationAIService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _model;

    public LocationAIService(
        IConfiguration configuration,
        ILogger<LocationAIService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        
        _apiKey = _configuration["OpenAI:ApiKey"];
        _model = _configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("OpenAI API key is not configured. Location AI service will not be available.");
        }
        else
        {
            _logger.LogInformation("Location AI service initialized - Model: {Model}", _model);
        }
    }

    public async Task<string> AskAboutLocationAsync(
        string question,
        string city,
        string? district,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured. Location AI service is unavailable.");
        }

        var locationInfo = district != null 
            ? $"{city}, {district} district"
            : city;

        var prompt = $@"You are a helpful assistant providing SPECIFIC and DETAILED location information in Poland, specifically in Warsaw and surrounding areas.

Location Information:
- City: {city}
{(district != null ? $"- District: {district}" : "")}
- Coordinates: {latitude}, {longitude}

User Question: {question}

CRITICAL REQUIREMENTS - YOU MUST FOLLOW THESE:
1. Answer ONLY what the user asked about. Do not provide information about other topics.
2. ALWAYS provide EXACT street names, addresses, and specific place names. Never use vague descriptions.
3. For each location, include:
   - Full name of the place (e.g., ""Przedszkole Miejskie nr 15"", ""Szkoła Podstawowa nr 123"")
   - Complete address with street name and number (e.g., ""ul. Nowy Świat 25"", ""al. Jerozolimskie 123"")
   - Approximate distance from the coordinates if possible (e.g., ""approximately 300 meters away"", ""about 1.2 km from location"")
4. Use Polish street naming conventions: ""ul."" for ulica, ""al."" for aleja, ""pl."" for plac
5. List items in a clear, numbered or bulleted format
6. If you don't know exact addresses, provide street names and approximate locations
7. NEVER use phrases like ""various facilities"", ""within a short distance"", ""nearby areas"", ""some places"", or ""various options""
8. Be specific: Instead of ""shops nearby"", say ""Carrefour at ul. Marszałkowska 15 (500m away)""

Example of EXCELLENT answer format:
""Near this location you can find:

1. Przedszkole Miejskie nr 15 at ul. Nowy Świat 25 (approximately 300 meters away)
2. Szkoła Podstawowa nr 123 at ul. Marszałkowska 45 (about 500 meters)
3. Park Saski at ul. Królewska 1 (1.2 km from location)
4. Metro station Centrum at al. Jerozolimskie 54 (400 meters away)""

Now provide a detailed answer with EXACT addresses and street names:";

        try
        {
            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant providing location information for real estate listings in Poland." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 800
            };

            var requestUri = new Uri("https://api.openai.com/v1/chat/completions");
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            _logger.LogInformation(
                "[LOCATION-AI] Asking about location - Question: {Question}, Location: {Location}, Coordinates: {Lat}, {Lon}",
                question, locationInfo, latitude, longitude);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "[LOCATION-AI] OpenAI API error (after retries) - StatusCode: {StatusCode}, ErrorContent: {ErrorContent}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"AI service is currently unavailable. Please try again later.");
            }

            var responseContent = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken: cancellationToken);
            var answer = responseContent?.Choices?.FirstOrDefault()?.Message?.Content ?? "I'm sorry, I couldn't generate a response. Please try again.";

            _logger.LogInformation(
                "[LOCATION-AI] Received answer - Question: {Question}, AnswerLength: {Length}",
                question, answer.Length);

            return answer;
        }
        catch (HttpRequestException)
        {
            // Re-throw HttpRequestException as-is (already has user-friendly message)
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex,
                "[LOCATION-AI] Request timeout (after retries) - Question: {Question}, Location: {Location}",
                question, locationInfo);
            throw new HttpRequestException("AI service request timed out. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[LOCATION-AI] Unexpected error (after retries) - Question: {Question}, Location: {Location}",
                question, locationInfo);
            throw new HttpRequestException("AI service is currently unavailable. Please try again later.");
        }
    }

    // OpenAI API response models
    private class OpenAIResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }
}

