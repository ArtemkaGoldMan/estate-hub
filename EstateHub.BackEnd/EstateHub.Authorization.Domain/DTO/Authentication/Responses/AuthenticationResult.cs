namespace EstateHub.Authorization.Domain.DTO.Authentication.Responses;

public class AuthenticationResult : AuthenticationResponse
{
    public string RefreshToken { get; set; }
}