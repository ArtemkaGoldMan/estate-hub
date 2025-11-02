using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.DataAccess.SqlServer.Db;
using EstateHub.ListingService.DataAccess.SqlServer.Repositories;
using EstateHub.ListingService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        // Add Infrastructure Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        // Register GridFS storage service (internal)
        services.AddScoped<Services.MongoGridFSStorageService>();
        
        // Register photo storage service (implements IPhotoStorageService)
        services.AddScoped<Domain.Interfaces.IPhotoStorageService, Services.PhotoStorageService>();

        return services;
    }
}
