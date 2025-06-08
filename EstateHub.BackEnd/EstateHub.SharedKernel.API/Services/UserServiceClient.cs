using System.Text;
using System.Text.Json;
using EstateHub.SharedKernel.API.Interfaces;
using EstateHub.SharedKernel.API.MicroserviceEndpoints;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Requests;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EstateHub.SharedKernel.API.Services;

public class UserServiceClient : IUserServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<UserServiceClient> _logger;
    private const string HttpClientName = "AuthServiceClient";

    private HttpRequest? CurrentHttpRequest => _httpContextAccessor.HttpContext?.Request;

    public UserServiceClient(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserServiceClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<UserIdFromTokenResponse?> GetUserIdFromTokenAsync()
    {
        var httpRequest = CurrentHttpRequest;
        if (httpRequest == null)
        {
            _logger.LogWarning("HttpContext is not available for {OperationName}", nameof(GetUserIdFromTokenAsync));
            return null;
        }

        return await ExecuteRequestAsync<UserIdFromTokenResponse>(
            HttpMethod.Get,
            AuthorizationMicroservice.GetUserIdFromToken,
            httpRequest,
            nameof(GetUserIdFromTokenAsync));
    }

    public async Task<GetUserResponse?> GetUserByIdAsync(Guid id, bool includeDeleted = false)
    {
        var httpRequest = CurrentHttpRequest;
        if (httpRequest == null)
        {
            _logger.LogWarning("HttpContext is not available for {OperationName}", nameof(GetUserByIdAsync));
            return null;
        }

        var uri = AuthorizationMicroservice.GetUserById
            .Replace("{id:guid}", id.ToString())
            .WithIncludeDeleted(includeDeleted);

        return await ExecuteRequestAsync<GetUserResponse>(
            HttpMethod.Get,
            uri,
            httpRequest,
            nameof(GetUserByIdAsync));
    }

    public async Task<GetUsersByIdsResponse?> GetUsersByIdsAsync(
        GetUsersByIdsRequest getUsersByIdsRequest,
        bool includeDeleted = false)
    {
        var httpRequest = CurrentHttpRequest;
        if (httpRequest == null)
        {
            _logger.LogWarning("HttpContext is not available for {OperationName}", nameof(GetUsersByIdsAsync));
            return null;
        }

        var uri = AuthorizationMicroservice.GetUsersByIds.WithIncludeDeleted(includeDeleted);

        return await ExecuteRequestAsync<GetUsersByIdsResponse>(
            HttpMethod.Post,
            uri,
            httpRequest,
            nameof(GetUsersByIdsAsync),
            getUsersByIdsRequest);
    }

    private async Task<T?> ExecuteRequestAsync<T>(
        HttpMethod method,
        string uri,
        HttpRequest originalRequest,
        string operationName,
        object? requestBody = null) where T : class
    {
        using var httpClient = _httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(method, uri);

        try
        {
            CopyHeaders(originalRequest, request);

            if (requestBody != null)
            {
                var jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            _logger.LogDebug("Executing {OperationName} request to {Uri}", operationName, uri);

            using var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);

                _logger.LogDebug("{OperationName} completed successfully", operationName);
                return result;
            }

            _logger.LogWarning("{OperationName} failed with status code {StatusCode}: {ReasonPhrase}",
                operationName, response.StatusCode, response.ReasonPhrase);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred during {OperationName}", operationName);
            return null;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout occurred during {OperationName}", operationName);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error during {OperationName}", operationName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during {OperationName}", operationName);
            return null;
        }
    }

    private static readonly HashSet<string> RestrictedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Host",
        "Content-Length",
        "Transfer-Encoding",
        "Connection",
        "Upgrade",
        "Proxy-Connection"
    };

    private static void CopyHeaders(HttpRequest originalRequest, HttpRequestMessage request)
    {
        foreach (var header in originalRequest.Headers)
        {
            if (!RestrictedHeaders.Contains(header.Key))
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }
    }
}
