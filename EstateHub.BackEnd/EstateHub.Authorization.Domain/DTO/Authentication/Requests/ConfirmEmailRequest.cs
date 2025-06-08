namespace EstateHub.Authorization.Domain.DTO.Authentication.Requests;

public class ConfirmEmailRequest
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}