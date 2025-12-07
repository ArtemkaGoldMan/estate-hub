using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Core.Services;
using EstateHub.ListingService.Core.Validators;
using EstateHub.ListingService.Core.Mappers;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EstateHub.ListingService.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddListingCore(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IListingService, Services.ListingService>();
        services.AddScoped<IReportService, Services.ReportService>();
        services.AddScoped<IPhotoService, Services.PhotoService>();
        services.AddScoped<IModerationService, Services.ModerationService>();
        services.AddScoped<IAIQuestionUsageService, Services.AIQuestionUsageService>();
        
        // Register background services
        services.AddSingleton<Services.BackgroundModerationService>();
        
        // Register mappers
        services.AddScoped<ReportDtoMapper>();
        services.AddScoped<ListingDtoMapper>();
        
        // Register validators
        services.AddValidatorsFromAssemblyContaining<CreateListingInputValidator>();
        
        return services;
    }
}
