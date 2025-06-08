namespace EstateHub.Authorization.Domain.Options;

public class JWTOptions
{
    public const string JWT = "JWT";

    public string Secret { get; set; } = string.Empty;
}
