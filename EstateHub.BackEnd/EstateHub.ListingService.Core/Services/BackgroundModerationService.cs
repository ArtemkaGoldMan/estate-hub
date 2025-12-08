using EstateHub.ListingService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace EstateHub.ListingService.Core.Services;

/// <summary>
/// Service for handling background moderation tasks with retry logic.
/// Ensures moderation checks are retried on failure and properly logged.
/// </summary>
public class BackgroundModerationService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BackgroundModerationService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public BackgroundModerationService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BackgroundModerationService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        // Configure retry policy: 3 attempts with exponential backoff
        // Retry on any exception except UnauthorizedAccessException (which indicates auth issues)
        _retryPolicy = Policy
            .Handle<Exception>(ex => !(ex is UnauthorizedAccessException))
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2s, 4s, 8s
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    var listingId = context.ContainsKey("ListingId") ? context["ListingId"]?.ToString() : "Unknown";
                    _logger.LogWarning(
                        "[MODERATION-BG-RETRY] Retry attempt {RetryCount}/3 for listing {ListingId} after {Delay}s. Error: {ErrorType} - {ErrorMessage}",
                        retryCount, listingId, timeSpan.TotalSeconds, exception.GetType().Name, exception.Message);
                });
    }

    /// <summary>
    /// Enqueues a moderation check for a listing with retry logic.
    /// This method returns immediately and processes the moderation in the background.
    /// </summary>
    public void EnqueueModerationCheck(Guid listingId, string context = "create")
    {
        _ = Task.Run(async () =>
        {
            var backgroundTaskId = Guid.NewGuid();
            _logger.LogInformation(
                "[MODERATION-BG-{TaskId}] ===== ENQUEUING BACKGROUND MODERATION ===== ListingId: {ListingId}, Context: {Context}, ThreadId: {ThreadId}, Timestamp: {Timestamp}",
                backgroundTaskId, listingId, context, Thread.CurrentThread.ManagedThreadId, DateTime.UtcNow);

            var contextData = new Context
            {
                ["ListingId"] = listingId.ToString(),
                ["TaskId"] = backgroundTaskId.ToString(),
                ["Context"] = context
            };

            try
            {
                await _retryPolicy.ExecuteAsync(async (ctx) =>
                {
                    await ExecuteModerationCheckAsync(listingId, backgroundTaskId, context);
                }, contextData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[MODERATION-BG-{TaskId}] ===== BACKGROUND MODERATION FAILED AFTER ALL RETRIES ===== ListingId: {ListingId}, Context: {Context}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}, Timestamp: {Timestamp}",
                    backgroundTaskId, listingId, context, ex.GetType().Name, ex.Message, DateTime.UtcNow);

            }
        });
    }

    private async Task ExecuteModerationCheckAsync(Guid listingId, Guid taskId, string context)
    {
        _logger.LogInformation(
            "[MODERATION-BG-{TaskId}] ===== STARTING MODERATION EXECUTION ===== ListingId: {ListingId}, Context: {Context}, Timestamp: {Timestamp}",
            taskId, listingId, context, DateTime.UtcNow);

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            try
            {
                _logger.LogDebug(
                    "[MODERATION-BG-{TaskId}] Service scope created - Starting moderation check - ListingId: {ListingId}",
                    taskId, listingId);

                var moderationService = scope.ServiceProvider.GetRequiredService<IModerationService>();

                var moderationStartTime = DateTime.UtcNow;
                await moderationService.CheckModerationAsync(listingId);
                var moderationDuration = DateTime.UtcNow - moderationStartTime;

                _logger.LogInformation(
                    "[MODERATION-BG-{TaskId}] ===== MODERATION EXECUTION COMPLETED SUCCESSFULLY ===== ListingId: {ListingId}, Context: {Context}, Duration: {Duration}ms, Timestamp: {Timestamp}",
                    taskId, listingId, context, moderationDuration.TotalMilliseconds, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[MODERATION-BG-{TaskId}] ===== MODERATION EXECUTION FAILED ===== ListingId: {ListingId}, Context: {Context}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}, Timestamp: {Timestamp}",
                    taskId, listingId, context, ex.GetType().Name, ex.Message, DateTime.UtcNow);

                // Re-throw to trigger retry policy
                throw;
            }
        }
    }
}


