using EstateHub.Authorization.Domain.Models;

namespace EstateHub.Authorization.Domain.DTO.Authentication.Requests;

public class ConfirmAccountActionRequest
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public AccountActionType ActionType { get; set; }
}
