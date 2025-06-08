using EstateHub.Authorization.Domain.Models;

namespace EstateHub.Authorization.Domain.DTO.Authentication.Requests;

public class ManageAccountRequest
{
    public string Email { get; set; }
    public AccountActionType ActionType { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
}
