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
using EstateHub.Authorization.Domain.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace EstateHub.Authorization.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection(JWTOptions.JWT));

        builder.Services.Configure<SmtpOptions>(
            builder.Configuration.GetSection(SmtpOptions.Smtp));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();

        // Add gRPC services
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection(); // For development

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
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedAccount = false;
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
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!)),
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
                policy.RequireRole("Admin", "Moderator"));
            
            options.AddPolicy(AuthorizationPolicies.UserAccess, policy => 
                policy.RequireRole("Admin", "Moderator", "User"));
            
            // Future policies for other microservices
            options.AddPolicy(AuthorizationPolicies.ListingManagement, policy => 
                policy.RequireRole("Admin", "Moderator", "User"));
            
            options.AddPolicy(AuthorizationPolicies.UserManagement, policy => 
                policy.RequireRole("Admin", "Moderator"));
            
            options.AddPolicy(AuthorizationPolicies.SystemSettings, policy => 
                policy.RequireRole("Admin"));
            
            options.AddPolicy(AuthorizationPolicies.AnalyticsAccess, policy => 
                policy.RequireRole("Admin", "Moderator"));
        });

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


        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors("Any");

        app.UseExceptionHandler();

        app.MapControllers();

        // Map gRPC services
        app.MapGrpcService<UserGrpcService>();
        app.MapGrpcReflectionService(); // For development

        app.Run();
    }
}
