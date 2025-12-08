using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.Models;

namespace EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;

/// <summary>
/// Service interface for identity and authentication-related operations.
/// Handles user registration, password management, email confirmation, and account actions using the underlying identity provider.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Verifies that the provided password matches the user's stored password.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="password">The password to verify.</param>
    /// <returns>A task that completes when the password check is done. Throws an exception if the password is incorrect.</returns>
    Task CheckPasswordAsync(Guid userId, string password);

    /// <summary>
    /// Registers a new user account in the identity provider system.
    /// </summary>
    /// <param name="request">The user model containing registration information (email, password, etc.).</param>
    /// <returns>A task that returns the user registration DTO containing the created user information.</returns>
    Task<UserRegistrationDto> RegisterAsync(User request);

    /// <summary>
    /// Confirms a user's email address using a confirmation token.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="token">The email confirmation token.</param>
    /// <returns>A task that completes when the email confirmation is processed.</returns>
    Task ConfirmEmailAsync(Guid userId, string token);

    /// <summary>
    /// Generates a password reset token for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting a password reset.</param>
    /// <returns>A task that returns the generated password reset token.</returns>
    Task<string> GeneratePasswordResetTokenAsync(Guid userId);

    /// <summary>
    /// Generates an account action token for operations like account deletion confirmation.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="actionType">The type of account action (e.g., DeleteAccount, LockAccount).</param>
    /// <returns>A task that returns the generated account action token.</returns>
    Task<string> GenerateAccountActionToken(Guid userId, AccountActionType actionType);

    /// <summary>
    /// Confirms an account action using a confirmation token.
    /// </summary>
    /// <param name="requestUserId">The unique identifier of the user performing the action.</param>
    /// <param name="requestToken">The account action confirmation token.</param>
    /// <param name="requestActionType">The type of account action being confirmed.</param>
    /// <returns>A task that completes when the account action confirmation is processed.</returns>
    Task ConfirmAccountAction(Guid requestUserId, string requestToken, AccountActionType requestActionType);

    /// <summary>
    /// Resets a user's password using a reset token.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="token">The password reset token.</param>
    /// <param name="password">The new password to set.</param>
    /// <returns>A task that completes when the password reset is processed.</returns>
    Task ResetPasswordAsync(Guid userId, string token, string password);
}
