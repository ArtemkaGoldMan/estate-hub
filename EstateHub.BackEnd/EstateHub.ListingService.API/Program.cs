using EstateHub.ListingService.API.Types.Queries;
using EstateHub.ListingService.API.Types.Mutations;
using EstateHub.ListingService.API.Types.InputTypes;
using EstateHub.ListingService.API.Types.OutputTypes;
using EstateHub.ListingService.Core.Extensions;
using EstateHub.ListingService.Infrastructure.Extensions;
using EstateHub.SharedKernel.API.Middleware;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

        // Add Data Access
        builder.Services.AddListingServiceDataAccess(builder.Configuration);
        
        // Add Core Services
        builder.Services.AddListingCore();

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
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!)),
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
            .AddType<AddPhotoInputType>()
            .AddType<ReorderPhotosInputType>()
            .AddAuthorization()
            .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Any", corsPolicyBuilder =>
            {
                corsPolicyBuilder
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // Add Seed service
        builder.Services.AddTransient<Seed>();

        var app = builder.Build();

        // Run database migrations
        await SeedData(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseCors("Any");
        
        // Enable static file serving for uploaded photos
        app.UseStaticFiles();
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseExceptionHandler();

        app.MapControllers();

        // Map GraphQL endpoint
        app.MapGraphQL("/graphql");

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
