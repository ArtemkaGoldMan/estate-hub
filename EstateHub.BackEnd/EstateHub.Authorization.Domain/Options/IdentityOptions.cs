namespace EstateHub.Authorization.Domain.Options;

/// <summary>
/// Configuration options for ASP.NET Identity settings.
/// </summary>
public class IdentityOptions
{
    public const string Identity = "Identity";

    /// <summary>
    /// Maximum number of failed login attempts before account lockout.
    /// Default: 5
    /// </summary>
    public int MaxFailedAccessAttempts { get; set; } = 5;

    /// <summary>
    /// Duration of account lockout after max failed attempts.
    /// Default: 15 minutes
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Whether lockout is enabled for new users.
    /// Default: true
    /// </summary>
    public bool AllowedForNewUsers { get; set; } = true;
}


