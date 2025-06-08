namespace EstateHub.Authorization.Domain.DTO.Authentication.Responses;

public class AuthenticationResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; }

    public string DisplayName { get; set; }

    public string Role { get; set; }

    public string AccessToken { get; set; }

    public string Avatar { get; set; }
}
