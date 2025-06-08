using System.Security.Claims;
using System.Text.Encodings.Web;
using EstateHub.SharedKernel.API.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EstateHub.SharedKernel.API;

public class MicroserviceAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserServiceClient _userServiceClient;

    public MicroserviceAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserServiceClient userServiceClient)
        : base(options, logger, encoder)
    {
        _userServiceClient = userServiceClient;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var userResponse = await _userServiceClient.GetUserIdFromTokenAsync();

            if (!IsValidUserResponse(userResponse))
            {
                Logger.LogWarning("Failed to get user ID from authorization microservice");
                return AuthenticateResult.Fail("Invalid or missing token");
            }

            var ticket = CreateAuthenticationTicket(userResponse.UserId);

            Logger.LogDebug("Successfully authenticated user {UserId}", userResponse.UserId);
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during authentication");
            return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
        }
    }

    private static bool IsValidUserResponse(dynamic userResponse)
    {
        return userResponse?.UserId != null;
    }

    private AuthenticationTicket CreateAuthenticationTicket(object userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()!) };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationTicket(principal, Scheme.Name);
    }
}
