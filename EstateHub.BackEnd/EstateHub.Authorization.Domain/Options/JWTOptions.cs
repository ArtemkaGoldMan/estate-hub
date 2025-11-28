namespace EstateHub.Authorization.Domain.Options;

public class JWTOptions
{
    public const string JWT = "JWT";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 10;
}
