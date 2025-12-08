using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Models;
using EstateHub.Authorization.Domain.Options;

namespace EstateHub.Authorization.Domain.Interfaces.InfrastructureInterfaces;

/// <summary>
/// Service interface for sending emails via SMTP.
/// Provides methods for sending various types of transactional emails including password reset, email confirmation, and account action notifications.
/// </summary>
public interface IEmailSmtpService
{
    /// <summary>
    /// Sends a password reset email to the user.
    /// </summary>
    /// <param name="options">SMTP configuration options (server, port, credentials, etc.).</param>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="token">The password reset token to include in the email.</param>
    /// <param name="returnUrl">The URL to redirect the user to after resetting their password.</param>
    /// <param name="userId">The unique identifier of the user requesting the password reset.</param>
    /// <returns>A Result indicating whether the email was successfully sent.</returns>
    Task<Result> SendForgetPasswordToken(SmtpOptions options, string email, string token, string returnUrl, Guid userId);

    /// <summary>
    /// Sends an email confirmation email to the user.
    /// </summary>
    /// <param name="options">SMTP configuration options (server, port, credentials, etc.).</param>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="token">The email confirmation token to include in the email.</param>
    /// <param name="returnUrl">The URL to redirect the user to after confirming their email.</param>
    /// <param name="userId">The unique identifier of the user to confirm.</param>
    /// <returns>A Result indicating whether the email was successfully sent.</returns>
    Task<Result> SendEmailConfirmationAsync(SmtpOptions options, string email, string token, string returnUrl, Guid userId);

    /// <summary>
    /// Sends an account action confirmation email to the user.
    /// Used for operations like account deletion, account locking, etc.
    /// </summary>
    /// <param name="smtpOptions">SMTP configuration options (server, port, credentials, etc.).</param>
    /// <param name="userResultEmail">The recipient's email address.</param>
    /// <param name="token">The account action confirmation token to include in the email.</param>
    /// <param name="returnUrl">The URL to redirect the user to after confirming the account action.</param>
    /// <param name="actionType">The type of account action (e.g., DeleteAccount, LockAccount).</param>
    /// <param name="userResultId">The unique identifier of the user performing the action.</param>
    /// <returns>A Result indicating whether the email was successfully sent.</returns>
    Task<Result> SendAccountActionToken(SmtpOptions smtpOptions, string userResultEmail, string token, string returnUrl, AccountActionType actionType, Guid userResultId);
}
