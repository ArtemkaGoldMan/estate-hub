using EstateHub.ListingService.Core.Abstractions;
using EstateHub.ListingService.Core.UseCases;
using EstateHub.ListingService.Core.Validators;
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
        services.AddScoped<IAdminService, UseCases.AdminService>();
        
        // Register validators
        services.AddValidatorsFromAssemblyContaining<CreateListingInputValidator>();
        
        return services;
    }
}
