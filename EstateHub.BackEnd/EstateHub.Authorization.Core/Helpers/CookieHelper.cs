using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace EstateHub.Authorization.Core.Helpers;

/// <summary>
/// Helper class for managing authentication cookies with secure settings.
/// </summary>
public static class CookieHelper
{
    /// <summary>
    /// Creates secure cookie options for refresh tokens.
    /// </summary>
    /// <param name="environment">The hosting environment to determine if we're in development.</param>
    /// <returns>Cookie options with appropriate security settings.</returns>
    public static CookieOptions CreateRefreshTokenCookieOptions(IHostEnvironment environment)
    {
        return new CookieOptions
        {
            // Secure flag: true in production, false in development (for localhost HTTP)
            Secure = !environment.IsDevelopment(),
            HttpOnly = true, // Prevent JavaScript access (XSS protection)
            SameSite = SameSiteMode.Lax, // CSRF protection while allowing cross-site navigation
            // Optional: Set expiration to match refresh token expiration
            // MaxAge = TimeSpan.FromDays(30)
        };
    }
}

