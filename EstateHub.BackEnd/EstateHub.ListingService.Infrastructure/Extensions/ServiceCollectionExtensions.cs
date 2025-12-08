using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.DataAccess.SqlServer.Db;
using EstateHub.ListingService.DataAccess.SqlServer.Repositories;
using EstateHub.ListingService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace EstateHub.ListingService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddListingServiceDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Add Repositories
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<ILikedListingRepository, LikedListingRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IAIQuestionUsageRepository, AIQuestionUsageRepository>();
        
        // Add UnitOfWork
        services.AddScoped<EstateHub.SharedKernel.Execution.IUnitOfWork, ListingUnitOfWork>();

        // Add Infrastructure Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        // Register GridFS storage service (internal)
        services.AddScoped<Services.MongoGridFSStorageService>();
        
        // Register photo storage service (implements IPhotoStorageService)
        services.AddScoped<Domain.Interfaces.IPhotoStorageService, Services.PhotoStorageService>();
        
        // Retry policy: 3 attempts with exponential backoff
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2s, 4s, 8s
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Logging will be handled by the service
                });

        // Register HttpClient for moderation service with timeout and retry policy
        services.AddHttpClient<Domain.Interfaces.IContentModerationService, Services.ContentModerationService>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(retryPolicy);

        // Register HttpClient for location AI service with timeout and retry policy
        services.AddHttpClient<Domain.Interfaces.ILocationAIService, Services.LocationAIService>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(60); // Longer timeout for AI responses
            })
            .AddPolicyHandler(retryPolicy);

        // Configure SMTP options
        services.Configure<Services.SmtpOptions>(
            configuration.GetSection(Services.SmtpOptions.Smtp));

        // Register email notification service
        services.AddScoped<Domain.Interfaces.IListingNotificationService, Services.ListingNotificationService>();

        return services;
    }
}
