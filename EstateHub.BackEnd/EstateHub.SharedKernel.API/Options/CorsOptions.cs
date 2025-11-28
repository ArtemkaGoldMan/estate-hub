namespace EstateHub.SharedKernel.API.Options;

public class CorsOptions
{
    public const string Cors = "Cors";

    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public bool AllowLocalhost { get; set; } = true;
}

