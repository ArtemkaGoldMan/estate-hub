using EstateHub.ListingService.API.Types.Queries;
using EstateHub.ListingService.API.Types.Mutations;
using EstateHub.ListingService.API.Types.InputTypes;
using EstateHub.ListingService.API.Types.OutputTypes;
using EstateHub.ListingService.Core.Extensions;
using EstateHub.ListingService.Infrastructure.Extensions;
using EstateHub.SharedKernel.API.Extensions;
using EstateHub.SharedKernel.API.Middleware;
using EstateHub.ListingService.Domain.Enums;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using EstateHub.SharedKernel.API.Options;
using Serilog;
using System.Text;

namespace EstateHub.ListingService.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        builder.Host.UseSerilog();

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // Add Data Access
        builder.Services.AddListingServiceDataAccess(builder.Configuration);
        
        // Add Core Services
        builder.Services.AddListingCore();

        // Add gRPC services for fetching user information from Authorization Service
        builder.Services.AddGrpcServices(builder.Configuration);

        // Add Authentication & Authorization with local JWT validation
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                
                var jwtIssuer = builder.Configuration["JWT:Issuer"];
                var jwtAudience = builder.Configuration["JWT:Audience"];
                var jwtSecret = builder.Configuration["JWT:Secret"]!;
                
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
                    ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret)),
                };

                // Optional: Add session validation for enhanced security
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        // Extract session ID from token if needed for additional validation
                        var sessionIdClaim = context.Principal?.Claims
                            .FirstOrDefault(c => c.Type == "sessionId");

                        if (sessionIdClaim != null)
                        {
                            // Session ID is available in the token for additional validation if needed
                            // For now, we'll just log it for debugging
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<Program>>();
                            logger.LogDebug("Token validated for session: {SessionId}", sessionIdClaim.Value);
                        }
                    },
                };
            });

        builder.Services.AddAuthorization();

        // Add GraphQL
        builder.Services
            .AddGraphQLServer()
            .AddQueryType<Queries>()
            .AddType<ReportQueries>()
            .AddType<PhotoQueries>()
            .AddMutationType<Mutations>()
            .AddType<ReportMutations>()
            .AddType<PhotoMutations>()
            .AddType<ListingType>()
            .AddType<PagedListingsType>()
            .AddType<CreateListingInputType>()
            .AddType<UpdateListingInputType>()
            .AddType<ChangeStatusInputType>()
            .AddType<PaginationInputType>()
            .AddType<BoundsInputType>()
            .AddType<ListingFilterType>()
            .AddType<ReportType>()
            .AddType<PagedReportsType>()
            .AddType<CreateReportInputType>()
            .AddType<ResolveReportInputType>()
            .AddType<DismissReportInputType>()
            .AddType<ReportFilterType>()
            .AddType<PhotoType>()
            .AddAuthorization()
            .AddType<UploadType>()
            .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

        // Configure CORS
        builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.Cors));
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Production", corsPolicyBuilder =>
            {
                var corsConfig = builder.Configuration.GetSection(CorsOptions.Cors).Get<CorsOptions>();
                var allowedOrigins = corsConfig?.AllowedOrigins ?? Array.Empty<string>();
                
                if (builder.Environment.IsDevelopment() && corsConfig?.AllowLocalhost == true)
                {
                    corsPolicyBuilder
                        .SetIsOriginAllowed(origin => 
                            allowedOrigins.Contains(origin) || 
                            origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) ||
                            origin.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase))
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
                else
                {
                    corsPolicyBuilder
                        .SetIsOriginAllowed(origin => allowedOrigins.Contains(origin))
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            });
        });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // Add Rate Limiting
        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.AddPolicy("graphql", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 50,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });

        // Add Seed service
        builder.Services.AddTransient<Seed>();

        var app = builder.Build();

        // Run database migrations
        await SeedData(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseForwardedHeaders();

        var enforceHttpsRedirect = builder.Configuration.GetValue<bool?>("EnforceHttpsRedirect") ?? true;

        if (enforceHttpsRedirect)
        {
            app.UseHttpsRedirection();
        }
        
        app.UseRateLimiter();
        app.UseCors("Production");
        
        // Static file serving is kept for backward compatibility with old locally-stored files
        // New photos are served via /api/photo/gridfs/{fileId} endpoint from MongoDB GridFS
        app.UseStaticFiles();
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseExceptionHandler();

        app.MapControllers();

        // Map GraphQL endpoint
        app.MapGraphQL("/graphql")
            .RequireRateLimiting("graphql");

        app.Run();
    }

    private static async Task SeedData(WebApplication app)
    {
        var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

        using (var scope = scopedFactory.CreateScope())
        {
            var service = scope.ServiceProvider.GetService<Seed>();
            await service.SeedDataContextAsync();
        }
    }
}
