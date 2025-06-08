using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.Authentication.Responses;
using Result = CSharpFunctionalExtensions.Result;

namespace EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;

public interface IAuthenticationService
{
    Task<Result<AuthenticationResult>> LoginAsync(LoginRequest request, bool beginTransaction = true);
    Task<Result<AuthenticationResult?>> RegisterAsync(UserRegistrationRequest request);
    Task<Result<AuthenticationResponse>> RefreshAccessTokenAsync(string refreshToken);
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<Result<AuthenticationResult>> ConfirmEmailAsync(ConfirmEmailRequest request);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
    Task<Result> ManageAccountState(ManageAccountRequest request);
    Task<Result> ConfirmAccountAction(ConfirmAccountActionRequest request);
    Task<Result> LogoutAsync(string refreshToken);
}
