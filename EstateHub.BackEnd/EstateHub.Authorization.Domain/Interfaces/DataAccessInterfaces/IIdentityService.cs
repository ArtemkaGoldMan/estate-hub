using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.Models;

namespace EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;

public interface IIdentityService
{
    Task CheckPasswordAsync(Guid userId, string password);
    Task<UserRegistrationDto> RegisterAsync(User request);
    Task ConfirmEmailAsync(Guid userId, string token);
    Task<string> GeneratePasswordResetTokenAsync(Guid userId);
    Task<string> GenerateAccountActionToken(Guid userId, AccountActionType actionType);
    Task ConfirmAccountAction(Guid requestUserId, string requestToken, AccountActionType requestActionType);
    Task ResetPasswordAsync(Guid userId, string token, string password);
}
