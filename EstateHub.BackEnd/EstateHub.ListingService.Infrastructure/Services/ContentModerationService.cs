using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EstateHub.ListingService.Infrastructure.Services;

public class ContentModerationService : IContentModerationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContentModerationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly bool _isEnabled;
    private readonly bool _failOpen;
    private readonly string? _apiKey;
    private readonly string _model;

    public ContentModerationService(
        IConfiguration configuration,
        ILogger<ContentModerationService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        
        _apiKey = _configuration["OpenAI:ApiKey"];
        _model = _configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
        _isEnabled = _configuration.GetValue<bool>("Moderation:Enabled", true);
        _failOpen = _configuration.GetValue<bool>("Moderation:FailOpen", false);

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("OpenAI API key is not configured. Moderation will be disabled.");
            _isEnabled = false;
        }
        else
        {
            _logger.LogInformation("OpenAI moderation service initialized - Model: {Model}, Enabled: {Enabled}, FailOpen: {FailOpen}", 
                _model, _isEnabled, _failOpen);
            // Log masked API key for verification (first 10 chars only)
            var maskedKey = _apiKey.Length > 10 ? _apiKey.Substring(0, 10) + "..." : "***";
            _logger.LogDebug("OpenAI API key configured (masked): {MaskedKey}", maskedKey);
        }
    }

    public async Task<ModerationResult> ModerateAsync(
        string title, 
        string description, 
        CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
        {
            _logger.LogDebug("Content moderation is disabled. Approving automatically.");
            return new ModerationResult(true, null, null);
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("OpenAI API key is not configured. Moderation cannot proceed.");
            if (_failOpen)
            {
                return new ModerationResult(true, null, null);
            }
            return new ModerationResult(
                false, 
                "Moderation service is temporarily unavailable. Please try again later.",
                null);
        }

        var moderationStartTime = DateTime.UtcNow;
        _logger.LogInformation(
            "[OPENAI] ===== CONTENT MODERATION START ===== Title: '{Title}', DescriptionLength: {DescriptionLength}, Model: {Model}, Enabled: {Enabled}, FailOpen: {FailOpen}, Timestamp: {Timestamp}",
            title ?? "[NULL]", description?.Length ?? 0, _model, _isEnabled, _failOpen, moderationStartTime);

        try
        {
            _logger.LogDebug("[OPENAI] Building moderation prompt... Title length: {TitleLength}, Description length: {DescLength}",
                title?.Length ?? 0, description?.Length ?? 0);

            var promptBuildStart = DateTime.UtcNow;
            var prompt = BuildModerationPrompt(title, description);
            var promptBuildDuration = DateTime.UtcNow - promptBuildStart;
            
            _logger.LogDebug("[OPENAI] Prompt built in {Duration}ms - Model: {Model}, Prompt length: {PromptLength} characters",
                promptBuildDuration.TotalMilliseconds, _model, prompt.Length);
            
            _logger.LogDebug("[OPENAI] Prompt content (first 500 chars): {PromptPreview}",
                prompt.Length > 500 ? prompt.Substring(0, 500) + "..." : prompt);

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are a content moderator for a real estate platform. Respond only with valid JSON." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.3,
                max_tokens = 200,
                response_format = new { type = "json_object" }
            };

            _logger.LogInformation(
                "[OPENAI] Preparing HTTP request - Model: {Model}, Endpoint: https://api.openai.com/v1/chat/completions, MessagesCount: {MessagesCount}, MaxTokens: 200, Temperature: 0.3",
                _model, requestBody.messages.Length);

            // Use absolute URI since we can't rely on BaseAddress being set (HttpClient reuse)
            var requestUri = new Uri("https://api.openai.com/v1/chat/completions");
            
            // Create request message with authorization header
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            
            _logger.LogDebug("[OPENAI] HTTP request created - Method: POST, Uri: {Uri}, HasAuthHeader: {HasAuth}, ContentType: {ContentType}",
                requestUri, request.Headers.Authorization != null, request.Content?.Headers?.ContentType);
            
            _logger.LogInformation("[OPENAI] Sending request to OpenAI API... Timestamp: {Timestamp}", DateTime.UtcNow);
            var requestStartTime = DateTime.UtcNow;
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var requestDuration = DateTime.UtcNow - requestStartTime;
            
            _logger.LogInformation(
                "[OPENAI] OpenAI API response received - StatusCode: {StatusCode}, Status: {Status}, Duration: {Duration}ms, Timestamp: {Timestamp}",
                response.StatusCode, response.StatusCode.ToString(), requestDuration.TotalMilliseconds, DateTime.UtcNow);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "[OPENAI] ===== OPENAI API ERROR ===== StatusCode: {StatusCode}, Status: {Status}, ErrorContent: {ErrorContent}, Duration: {Duration}ms",
                    response.StatusCode, response.StatusCode.ToString(), errorContent, requestDuration.TotalMilliseconds);
                throw new HttpRequestException($"OpenAI API error: {response.StatusCode}");
            }

            response.EnsureSuccessStatusCode();

            _logger.LogDebug("[OPENAI] Parsing response content...");
            var parseStartTime = DateTime.UtcNow;
            var responseContent = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken: cancellationToken);
            var parseDuration = DateTime.UtcNow - parseStartTime;
            
            var responseText = responseContent?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
            
            _logger.LogInformation(
                "[OPENAI] Response parsed in {ParseDuration}ms - Raw response length: {Length} characters, Choices count: {ChoicesCount}",
                parseDuration.TotalMilliseconds, responseText.Length, responseContent?.Choices?.Count ?? 0);
            
            _logger.LogInformation("[OPENAI] ===== GPT CHATGPT FULL RESPONSE ===== {Response}", responseText);
            _logger.LogDebug("[OPENAI] Response content preview (first 500 chars): {ResponsePreview}",
                responseText.Length > 500 ? responseText.Substring(0, 500) + "..." : responseText);

            _logger.LogDebug("[OPENAI] Parsing moderation response from JSON...");
            var moderationParseStart = DateTime.UtcNow;
            var result = ParseModerationResponse(responseText);
            var moderationParseDuration = DateTime.UtcNow - moderationParseStart;
            
            _logger.LogInformation(
                "[OPENAI] ===== MODERATION RESULT PARSED ===== ParseDuration: {ParseDuration}ms, Approved: {Approved}, HasReason: {HasReason}, HasSuggestions: {HasSuggestions}",
                moderationParseDuration.TotalMilliseconds, result.IsApproved, !string.IsNullOrEmpty(result.RejectionReason), result.Suggestions?.Any() ?? false);
            
            _logger.LogInformation(
                "[OPENAI] Moderation result details - Approved: {Approved}, Reason: '{Reason}', SuggestionsCount: {SuggestionsCount}",
                result.IsApproved, result.RejectionReason ?? "N/A", result.Suggestions?.Count ?? 0);
            
            if (result.Suggestions != null && result.Suggestions.Any())
            {
                _logger.LogInformation("[OPENAI] Moderation suggestions: [{Suggestions}]",
                    string.Join("; ", result.Suggestions));
            }
            
            var totalDuration = DateTime.UtcNow - moderationStartTime;
            _logger.LogInformation(
                "[OPENAI] ===== CONTENT MODERATION END ===== Total duration: {TotalDuration}ms, API call duration: {ApiDuration}ms, Result: {Result}, Timestamp: {Timestamp}",
                totalDuration.TotalMilliseconds, requestDuration.TotalMilliseconds, result.IsApproved ? "APPROVED" : "REJECTED", DateTime.UtcNow);

            return result;
        }
        catch (HttpRequestException ex)
        {
            var totalDuration = DateTime.UtcNow - moderationStartTime;
            _logger.LogError(ex,
                "[OPENAI] ===== HTTP REQUEST ERROR ===== ErrorType: {ErrorType}, Message: {Message}, Duration: {Duration}ms, StackTrace: {StackTrace}",
                ex.GetType().Name, ex.Message, totalDuration.TotalMilliseconds, ex.StackTrace);
            
            if (_failOpen)
            {
                _logger.LogWarning("[OPENAI] Moderation failed but fail-open is enabled. Approving automatically. Duration: {Duration}ms", totalDuration.TotalMilliseconds);
                return new ModerationResult(true, null, null);
            }

            return new ModerationResult(
                false,
                $"Moderation service error: {ex.Message}",
                null);
        }
        catch (TaskCanceledException ex)
        {
            var totalDuration = DateTime.UtcNow - moderationStartTime;
            _logger.LogError(ex,
                "[OPENAI] ===== REQUEST TIMEOUT ===== ErrorType: {ErrorType}, Message: {Message}, Duration: {Duration}ms, CancellationRequested: {CancellationRequested}",
                ex.GetType().Name, ex.Message, totalDuration.TotalMilliseconds, ex.CancellationToken.IsCancellationRequested);
            
            if (_failOpen)
            {
                _logger.LogWarning("[OPENAI] Request timeout but fail-open is enabled. Approving automatically.");
                return new ModerationResult(true, null, null);
            }

            return new ModerationResult(
                false,
                "Moderation request timed out. Please try again later.",
                null);
        }
        catch (JsonException ex)
        {
            var totalDuration = DateTime.UtcNow - moderationStartTime;
            _logger.LogError(ex,
                "[OPENAI] ===== JSON PARSING ERROR ===== ErrorType: {ErrorType}, Message: {Message}, Duration: {Duration}ms, Path: {Path}, LineNumber: {LineNumber}",
                ex.GetType().Name, ex.Message, totalDuration.TotalMilliseconds, ex.Path, ex.LineNumber);
            
            if (_failOpen)
            {
                _logger.LogWarning("[OPENAI] JSON parsing error but fail-open is enabled. Approving automatically.");
                return new ModerationResult(true, null, null);
            }

            return new ModerationResult(
                false,
                "Failed to parse moderation response. Please try again later.",
                null);
        }
        catch (Exception ex)
        {
            var totalDuration = DateTime.UtcNow - moderationStartTime;
            _logger.LogError(ex,
                "[OPENAI] ===== UNEXPECTED ERROR DURING CONTENT MODERATION ===== ErrorType: {ErrorType}, Message: {Message}, Duration: {Duration}ms, StackTrace: {StackTrace}",
                ex.GetType().Name, ex.Message, totalDuration.TotalMilliseconds, ex.StackTrace);
            
            if (_failOpen)
            {
                _logger.LogWarning("[OPENAI] Unexpected error but fail-open is enabled. Approving automatically.");
                return new ModerationResult(true, null, null);
            }

            return new ModerationResult(
                false,
                $"An unexpected error occurred during moderation: {ex.Message}",
                null);
        }
    }

    private string BuildModerationPrompt(string title, string description)
    {
        return $@"You are a content moderator for a real estate platform.

Please review the following property listing:

Title: {title}
Description: {description}

Rules:
1. Content must be related to real estate property listings
2. No spam, scams, or fraudulent content
3. No inappropriate, offensive, or discriminatory language
4. No contact information in title/description (phone, email, website URLs)
5. Title must be descriptive and relevant to the property
6. Description must provide meaningful property details
7. No excessive capitalization or spam-like text

Respond with ONLY a valid JSON object in this exact format:
{{
  ""approved"": true/false,
  ""reason"": ""reason if rejected (or null if approved)"",
  ""suggestions"": [""suggestion1"", ""suggestion2""] or null
}}";
    }

    private ModerationResult ParseModerationResponse(string responseText)
    {
        var parseStartTime = DateTime.UtcNow;
        _logger.LogDebug("[OPENAI-PARSE] Starting to parse moderation response... Response length: {Length} characters", responseText.Length);
        
        try
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                _logger.LogError("[OPENAI-PARSE] ===== EMPTY RESPONSE TEXT ===== Cannot parse empty response");
                return new ModerationResult(
                    false,
                    "Empty moderation response received.",
                    new List<string> { "Please review your listing content and try again." });
            }

            // Try to extract JSON from the response (in case there's extra text)
            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}') + 1;
            
            _logger.LogDebug("[OPENAI-PARSE] JSON extraction - Original length: {OriginalLength}, JsonStart: {JsonStart}, JsonEnd: {JsonEnd}",
                responseText.Length, jsonStart, jsonEnd);
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var originalLength = responseText.Length;
                responseText = responseText.Substring(jsonStart, jsonEnd - jsonStart);
                _logger.LogDebug("[OPENAI-PARSE] Extracted JSON substring - Original length: {OriginalLength}, Extracted length: {ExtractedLength}",
                    originalLength, responseText.Length);
            }
            else
            {
                _logger.LogWarning("[OPENAI-PARSE] No JSON brackets found in response. Attempting to parse entire response...");
            }

            _logger.LogDebug("[OPENAI-PARSE] Attempting JSON document parse...");
            using var doc = JsonDocument.Parse(responseText);
            var root = doc.RootElement;
            _logger.LogDebug("[OPENAI-PARSE] JSON document parsed successfully. Root element value kind: {ValueKind}", root.ValueKind);

            var approved = root.TryGetProperty("approved", out var approvedProp) && approvedProp.GetBoolean();
            _logger.LogDebug("[OPENAI-PARSE] 'approved' property - Found: {Found}, Value: {Value}",
                root.TryGetProperty("approved", out var _), approved);
            
            var reason = root.TryGetProperty("reason", out var reasonProp) 
                ? reasonProp.GetString() 
                : null;
            _logger.LogDebug("[OPENAI-PARSE] 'reason' property - Found: {Found}, Value: '{Value}'",
                root.TryGetProperty("reason", out var _), reason ?? "[NULL]");
            
            List<string>? suggestions = null;
            if (root.TryGetProperty("suggestions", out var suggestionsProp) && suggestionsProp.ValueKind == JsonValueKind.Array)
            {
                _logger.LogDebug("[OPENAI-PARSE] 'suggestions' property found as array. Parsing suggestions...");
                suggestions = suggestionsProp.EnumerateArray()
                    .Select(s => s.GetString())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList()!;
                _logger.LogDebug("[OPENAI-PARSE] Parsed {Count} suggestions from response", suggestions.Count);
            }
            else
            {
                _logger.LogDebug("[OPENAI-PARSE] 'suggestions' property - Found: {Found}, IsArray: {IsArray}",
                    root.TryGetProperty("suggestions", out var _), 
                    root.TryGetProperty("suggestions", out var sp) && sp.ValueKind == JsonValueKind.Array);
            }

            // If no suggestions but rejected, add a default suggestion
            if (!approved && (suggestions == null || suggestions.Count == 0))
            {
                _logger.LogDebug("[OPENAI-PARSE] Listing rejected but no suggestions provided. Adding default suggestion.");
                suggestions = new List<string> { "Please review the content and ensure it follows our guidelines." };
            }

            var parseDuration = DateTime.UtcNow - parseStartTime;
            _logger.LogInformation(
                "[OPENAI-PARSE] ===== PARSING COMPLETED SUCCESSFULLY ===== Duration: {Duration}ms, Approved: {Approved}, HasReason: {HasReason}, SuggestionsCount: {SuggestionsCount}",
                parseDuration.TotalMilliseconds, approved, !string.IsNullOrEmpty(reason), suggestions?.Count ?? 0);

            return new ModerationResult(approved, reason, suggestions);
        }
        catch (JsonException ex)
        {
            var parseDuration = DateTime.UtcNow - parseStartTime;
            _logger.LogError(ex,
                "[OPENAI-PARSE] ===== JSON PARSING EXCEPTION ===== Duration: {Duration}ms, ErrorType: {ErrorType}, Message: {Message}, Path: {Path}, LineNumber: {LineNumber}, BytePosition: {BytePosition}, ResponseText: {ResponseText}",
                parseDuration.TotalMilliseconds, ex.GetType().Name, ex.Message, ex.Path ?? "N/A", ex.LineNumber ?? 0, 
                ex.BytePositionInLine ?? 0, responseText);
            
            // If we can't parse, reject for safety
            return new ModerationResult(
                false,
                $"Unable to process moderation response: {ex.Message}",
                new List<string> { "Please review your listing content and try again." });
        }
        catch (Exception ex)
        {
            var parseDuration = DateTime.UtcNow - parseStartTime;
            _logger.LogError(ex,
                "[OPENAI-PARSE] ===== UNEXPECTED PARSING EXCEPTION ===== Duration: {Duration}ms, ErrorType: {ErrorType}, Message: {Message}, StackTrace: {StackTrace}, ResponseText: {ResponseText}",
                parseDuration.TotalMilliseconds, ex.GetType().Name, ex.Message, ex.StackTrace, responseText);
            
            // If we can't parse, reject for safety
            return new ModerationResult(
                false,
                $"Unable to process moderation response: {ex.Message}",
                new List<string> { "Please review your listing content and try again." });
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

