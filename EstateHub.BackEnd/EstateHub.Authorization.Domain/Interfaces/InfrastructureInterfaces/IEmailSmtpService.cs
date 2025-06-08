using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Models;
using EstateHub.Authorization.Domain.Options;

namespace EstateHub.Authorization.Domain.Interfaces.InfrastructureInterfaces;

public interface IEmailSmtpService
{
    Task<Result> SendForgetPasswordToken(SmtpOptions options, string email, string token, string returnUrl, Guid userId);
    Task<Result> SendEmailConfirmationAsync(SmtpOptions options, string email, string token, string returnUrl, Guid userId);
    Task<Result> SendAccountActionToken(SmtpOptions smtpOptions, string userResultEmail, string token, string returnUrl, AccountActionType actionType, Guid userResultId);
}
