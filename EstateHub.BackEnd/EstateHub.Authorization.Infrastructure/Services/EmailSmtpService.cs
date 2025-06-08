using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.InfrastructureInterfaces;
using EstateHub.Authorization.Domain.Models;
using EstateHub.Authorization.Domain.Options;
using MailKit.Net.Smtp;
using MimeKit;

namespace EstateHub.Authorization.Infrastructure.Services;

public class EmailSmtpService : IEmailSmtpService
{
    /*"smtp.gmail.com", 587*/

    public async Task<Result> SendForgetPasswordToken(SmtpOptions options, string email, string token, string returnUrl, Guid userId)
    {
        try
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("", options.User));
            emailMessage.To.Add(new MailboxAddress("", email));

            emailMessage.Subject = "Forget Password Token";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"<a href='{returnUrl}?token={token}&id={userId}'>Click here to reset your password</a>",
            };

            emailMessage.Body = bodyBuilder.ToMessageBody();

            await SendMessageAsync(
                options.User,
                options.Password,
                options.Host,
                options.Port,
                emailMessage);

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result> SendEmailConfirmationAsync(SmtpOptions options, string email, string token, string returnUrl, Guid userId)
    {
        try
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("", options.User));
            emailMessage.To.Add(new MailboxAddress("", email));

            emailMessage.Subject = "Email Confirmation";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"<a href='{returnUrl}?token={token}&id={userId}'>Click here to confirm your email</a>",
            };

            emailMessage.Body = bodyBuilder.ToMessageBody();

            await SendMessageAsync(
                options.User,
                options.Password,
                options.Host,
                options.Port,
                emailMessage);

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    public Task<Result> SendAccountActionToken(
        SmtpOptions smtpOptions,
        string userResultEmail,
        string token,
        string returnUrl,
        AccountActionType actionType,
        Guid userResultId)
    {
        try
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("", smtpOptions.User));
            emailMessage.To.Add(new MailboxAddress("", userResultEmail));

            emailMessage.Subject = actionType switch
            {
                AccountActionType.HardDelete => "Hard Delete",
                AccountActionType.Recover => "Recover",
                _ => throw new ArgumentException(AuthorizationErrors.NotFoundAccountAction().ToString())
            };

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"<a href='{returnUrl}?token={token}&id={userResultId}'>Click here to {actionType.ToString()}</a>",
            };

            emailMessage.Body = bodyBuilder.ToMessageBody();

            SendMessageAsync(
                smtpOptions.User,
                smtpOptions.Password,
                smtpOptions.Host,
                smtpOptions.Port,
                emailMessage);

            return Task.FromResult(Result.Success());
        }
        catch (Exception e)
        {
            return Task.FromResult(Result.Failure(e.Message));
        }
    }

    private static async Task SendMessageAsync(string user, string password, string smtpHost, int smtpPort,
        MimeMessage email)
    {
        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(smtpHost, smtpPort, false);
        await smtp.AuthenticateAsync(user, password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
