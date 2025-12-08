using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.Authentication.Responses;
using Result = CSharpFunctionalExtensions.Result;

namespace EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;

/// <summary>
/// Service interface for authentication operations.
/// Handles user login, registration, password management, email confirmation, and session management.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with email and password credentials.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <param name="beginTransaction">Whether to begin a database transaction for this operation. Defaults to true.</param>
    /// <returns>A Result containing authentication tokens and user information if login succeeds, or an error if authentication fails.</returns>
    Task<Result<AuthenticationResult>> LoginAsync(LoginRequest request, bool beginTransaction = true);

    /// <summary>
    /// Registers a new user account.
    /// If email confirmation is not required, the user is automatically logged in and AuthenticationResult is returned.
    /// If email confirmation is required, null is returned and a confirmation email is sent.
    /// </summary>
    /// <param name="request">The user registration request containing email, password, and confirmation callback URL.</param>
    /// <returns>
    /// A Result containing AuthenticationResult if auto-login is enabled (email confirmation not required),
    /// or null if email confirmation is required (confirmation email will be sent).
    /// </returns>
    Task<Result<AuthenticationResult?>> RegisterAsync(UserRegistrationRequest request);

    /// <summary>
    /// Refreshes an access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token used to obtain a new access token.</param>
    /// <returns>A Result containing a new AuthenticationResponse with updated tokens, or an error if the refresh token is invalid or expired.</returns>
    Task<Result<AuthenticationResponse>> RefreshAccessTokenAsync(string refreshToken);

    /// <summary>
    /// Initiates the password reset process by sending a reset token to the user's email.
    /// </summary>
    /// <param name="request">The forgot password request containing the user's email and return URL.</param>
    /// <returns>A Result indicating success or failure. Note: Returns success even if the user doesn't exist (security best practice).</returns>
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request);

    /// <summary>
    /// Confirms a user's email address using a confirmation token.
    /// </summary>
    /// <param name="request">The email confirmation request containing the token and user ID.</param>
    /// <returns>A Result containing AuthenticationResult with tokens if confirmation succeeds, or an error if the token is invalid or expired.</returns>
    Task<Result<AuthenticationResult>> ConfirmEmailAsync(ConfirmEmailRequest request);

    /// <summary>
    /// Resets a user's password using a reset token.
    /// </summary>
    /// <param name="request">The password reset request containing the token, user ID, and new password.</param>
    /// <returns>A Result indicating success or failure of the password reset operation.</returns>
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>
    /// Manages account state operations such as locking/unlocking accounts.
    /// </summary>
    /// <param name="request">The account management request containing the action to perform.</param>
    /// <returns>A Result indicating success or failure of the account state change.</returns>
    Task<Result> ManageAccountState(ManageAccountRequest request);

    /// <summary>
    /// Confirms an account action (e.g., account deletion confirmation) using a confirmation token.
    /// </summary>
    /// <param name="request">The account action confirmation request containing the token and user ID.</param>
    /// <returns>A Result indicating success or failure of the account action confirmation.</returns>
    Task<Result> ConfirmAccountAction(ConfirmAccountActionRequest request);

    /// <summary>
    /// Logs out a user by invalidating their refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to invalidate.</param>
    /// <returns>A Result indicating success or failure of the logout operation.</returns>
    Task<Result> LogoutAsync(string refreshToken);
}
