namespace EstateHub.Authorization.Domain.DTO.Session;

public class SessionDto
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
    
    public DateTimeOffset ExpirationDate { get; set; }
}