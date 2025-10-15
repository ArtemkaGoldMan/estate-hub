using EstateHub.ListingService.API.Types;
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

        // Add Authentication & Authorization
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
            });

        builder.Services.AddAuthorization();

        // Add GraphQL
        builder.Services
            .AddGraphQLServer()
            .AddQueryType<Queries>()
            .AddMutationType<Mutations>()
            .AddType<ListingType>()
            .AddType<PagedListingsType>()
            .AddType<CreateListingInputType>()
            .AddType<UpdateListingInputType>()
            .AddType<ChangeStatusInputType>()
            .AddType<PaginationInputType>()
            .AddType<BoundsInputType>()
            .AddType<ListingFilterType>()
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
