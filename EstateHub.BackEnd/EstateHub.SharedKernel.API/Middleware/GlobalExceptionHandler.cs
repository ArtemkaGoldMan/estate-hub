using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EstateHub.SharedKernel.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var (statusCode, title, errorCode) = MapExceptionToError(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        // Add error code if available
        if (!string.IsNullOrEmpty(errorCode))
        {
            problemDetails.Extensions["errorCode"] = errorCode;
        }

        // Add exception type for debugging (only in development)
        var environment = httpContext.RequestServices.GetService<IWebHostEnvironment>();
        if (environment?.EnvironmentName == "Development")
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, string? ErrorCode) MapExceptionToError(Exception exception)
    {
        // Check if exception has error code in Data dictionary
        if (exception.Data.Contains("ErrorCode") && exception.Data["ErrorCode"] is string errorCode)
        {
            // Extract status code from error code (first digit indicates HTTP status)
            var statusCode = GetStatusCodeFromErrorCode(errorCode);
            return (statusCode, GetTitleFromStatusCode(statusCode), errorCode);
        }

        // Map common exception types
        return exception switch
        {
            ArgumentException or ArgumentNullException => 
                (StatusCodes.Status400BadRequest, "Bad Request", null),
            KeyNotFoundException => 
                (StatusCodes.Status404NotFound, "Not Found", null),
            InvalidOperationException => 
                (StatusCodes.Status403Forbidden, "Forbidden", null),
            UnauthorizedAccessException => 
                (StatusCodes.Status403Forbidden, "Forbidden", null),
            _ => 
                (StatusCodes.Status500InternalServerError, "Internal Server Error", null)
        };
    }

    private static int GetStatusCodeFromErrorCode(string errorCode)
    {
        // Error codes follow pattern: XXXX
        // Listing Service: 2000-2999 range
        // First two digits indicate HTTP status family
        if (errorCode.Length >= 4 && int.TryParse(errorCode.Substring(0, 2), out var codePrefix))
        {
            return codePrefix switch
            {
                >= 2000 and < 2100 => StatusCodes.Status400BadRequest, // 2000-2099: Bad Request
                >= 2200 and < 2300 => StatusCodes.Status403Forbidden, // 2200-2299: Forbidden
                >= 2300 and < 2400 => StatusCodes.Status404NotFound, // 2300-2399: Not Found
                >= 2400 and < 2500 => StatusCodes.Status409Conflict, // 2400-2499: Conflict
                >= 2500 and < 2600 => StatusCodes.Status422UnprocessableEntity, // 2500-2599: Unprocessable Entity
                >= 2900 and < 3000 => StatusCodes.Status500InternalServerError, // 2900-2999: Internal Server Error
                _ => StatusCodes.Status500InternalServerError
            };
        }

        return StatusCodes.Status500InternalServerError;
    }

    private static string GetTitleFromStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
        StatusCodes.Status500InternalServerError => "Internal Server Error",
        _ => "Error"
    };
}
