using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Core.UseCases;
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
        services.AddScoped<IListingService, UseCases.ListingService>();
        services.AddScoped<IReportService, UseCases.ReportService>();
        services.AddScoped<IPhotoService, UseCases.PhotoService>();
        
        // Register mappers
        services.AddScoped<ReportDtoMapper>();
        services.AddScoped<ListingDtoMapper>();
        
        // Register validators
        services.AddValidatorsFromAssemblyContaining<CreateListingInputValidator>();
        
        return services;
    }
}
