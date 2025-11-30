using System.Text;
using EstateHub.Authorization.Domain;
using EstateHub.Authorization.Domain.DTO.Session;
using EstateHub.Authorization.Domain.Interfaces.ApplicationInterfaces;
using EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.Authorization.Domain.Interfaces.InfrastructureInterfaces;
using EstateHub.Authorization.Domain.Options;
using EstateHub.Authorization.Core.Helpers;
using EstateHub.Authorization.Core.Services;
using EstateHub.Authorization.Core.Services.Authentication;
using EstateHub.Authorization.DataAccess.SqlServer;
using EstateHub.Authorization.DataAccess.SqlServer.Entities;
using EstateHub.Authorization.DataAccess.SqlServer.Repositories;
using EstateHub.Authorization.DataAccess.SqlServer.Services;
using EstateHub.Authorization.Infrastructure.Services;
using EstateHub.SharedKernel.API.Middleware;
using EstateHub.SharedKernel.API.Options;
using EstateHub.Authorization.Domain.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;
using Swashbuckle.AspNetCore.Filters;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace EstateHub.Authorization.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        // Use Serilog for logging
        builder.Host.UseSerilog();

        builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection(JWTOptions.JWT));

        builder.Services.Configure<SmtpOptions>(
            builder.Configuration.GetSection(SmtpOptions.Smtp));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();
        
        // Configure request size limits for file uploads (avatar uploads)
        builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5MB for avatar uploads
            options.ValueLengthLimit = 5 * 1024 * 1024; // 5MB
            options.ValueCountLimit = 1024; // Maximum number of form values
            options.MultipartBoundaryLengthLimit = 128; // Boundary length limit
            options.MultipartHeadersCountLimit = 32; // Maximum number of headers
            options.MultipartHeadersLengthLimit = 16384; // 16KB headers
        });
        
        // Configure Kestrel server limits
        builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = 5 * 1024 * 1024; // 5MB for avatar uploads
            options.Limits.MaxRequestHeadersTotalSize = 32 * 1024; // 32KB
            options.Limits.MaxRequestHeaderCount = 100;
        });

        // Add gRPC services
        builder.Services.AddGrpc();
        // Only enable gRPC reflection in development
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddGrpcReflection();
        }

        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2",
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header, Name = "Authorization", Type = SecuritySchemeType.ApiKey,
                });

            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                x => x.MigrationsAssembly("EstateHub.Authorization.DataAccess.SqlServer")));

        builder.Services
            .AddIdentityCore<UserEntity>(options =>
            {
                options.User.RequireUniqueEmail = true;
                // Strong password requirements
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 3;
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddRoles<RoleEntity>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserManager<UserManager<UserEntity>>()
            .AddRoleManager<RoleManager<RoleEntity>>()
            .AddDefaultTokenProviders();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        });

        builder.Services.AddAutoMapper(config =>
        {
            config.AddProfile<DomainMappingProfile>();
            config.AddProfile<DataAccessMappingProfile>();
        });

        builder.Services.AddTransient<Seed>();

        builder.Services.AddScoped<IEmailSmtpService, EmailSmtpService>();

        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<IUsersService, UsersService>();
        builder.Services.AddScoped<ISessionsService, SessionsService>();

        builder.Services.AddScoped<IIdentityService, IdentityService>();

        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        builder.Services.AddScoped<ISessionsRepository, SessionsRepository>();
        builder.Services.AddScoped<IUnitOfWork, DatabaseUnitOfWork>();

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
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var sessionRepository = context.HttpContext.RequestServices
                            .GetRequiredService<ISessionsRepository>();

                        var sessionIdClaim = context.Principal?.Claims
                            .FirstOrDefault(c => c.Type == JwtHelper.SessionIdClaimName);

                        if (sessionIdClaim == null)
                        {
                            context.Fail("Session ID claim not found in token");
                            return;
                        }

                        var sessionIdStr = sessionIdClaim.Value;

                        if (!Guid.TryParse(sessionIdStr, out var sessionId))
                        {
                            context.Fail("Invalid session ID claim value");
                            return;
                        }

                        var session = await sessionRepository.GetByIdAsync<SessionDto>(sessionId);

                        if (session == null)
                        {
                            context.Fail("Invalid session");
                        }
                    },
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            // Core system policies
            options.AddPolicy(AuthorizationPolicies.AdminAccess, policy => 
                policy.RequireRole("Admin"));
            
            options.AddPolicy(AuthorizationPolicies.ModerationAccess, policy => 
                policy.RequireRole("Admin"));
            
            options.AddPolicy(AuthorizationPolicies.UserAccess, policy => 
                policy.RequireRole("Admin", "User"));
            
            // Future policies for other microservices
            options.AddPolicy(AuthorizationPolicies.ListingManagement, policy => 
                policy.RequireRole("Admin", "User"));
            
            options.AddPolicy(AuthorizationPolicies.UserManagement, policy => 
                policy.RequireRole("Admin"));
            
            options.AddPolicy(AuthorizationPolicies.SystemSettings, policy => 
                policy.RequireRole("Admin"));
            
            options.AddPolicy(AuthorizationPolicies.AnalyticsAccess, policy => 
                policy.RequireRole("Admin"));
        });

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

        // Add Health Checks
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("sqlserver", tags: new[] { "db", "sqlserver", "ready" })
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "self", "live" });

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

            options.AddPolicy("auth", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });

        var app = builder.Build();

        await SeedData(app);

        async Task SeedData(IHost app)
        {
            var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

            using (var scope = scopedFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<Seed>();
                await service.SeedDataContextAsync();
            }
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Conditionally enable HTTPS redirection (disabled for local development)
        var enforceHttpsRedirect = app.Configuration.GetValue<bool?>("EnforceHttpsRedirect") ?? false;
        if (enforceHttpsRedirect)
        {
            app.UseHttpsRedirection();
        }

        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors("Production");

        app.UseExceptionHandler();

        app.MapControllers()
            .RequireRateLimiting("auth");

        // Map Health Check endpoints
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Map gRPC services
        app.MapGrpcService<UserGrpcService>();
        // Only enable gRPC reflection in development
        if (app.Environment.IsDevelopment())
        {
            app.MapGrpcReflectionService();
        }

        app.Run();
    }
}
