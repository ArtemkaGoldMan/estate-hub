using EstateHub.SharedKernel.API.Interfaces;
using EstateHub.SharedKernel.API.Services;
using EstateHub.SharedKernel.Contracts.Grpc;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;

namespace EstateHub.SharedKernel.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authServiceGrpcUrl = configuration["AuthService:GrpcUrl"] ?? "https://localhost:7001";

        services.AddGrpcClient<UserService.UserServiceClient>(options =>
        {
            options.Address = new Uri(authServiceGrpcUrl);
        });

        services.AddScoped<IUserServiceClient, UserServiceGrpcClient>();

        return services;
    }

    public static IServiceCollection AddMicroserviceAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string schemeName = "MicroserviceAuth")
    {
        services.AddGrpcServices(configuration);

        services.AddAuthentication(schemeName)
            .AddScheme<AuthenticationSchemeOptions, MicroserviceAuthenticationHandler>(
                schemeName, options => { });

        return services;
    }
}
