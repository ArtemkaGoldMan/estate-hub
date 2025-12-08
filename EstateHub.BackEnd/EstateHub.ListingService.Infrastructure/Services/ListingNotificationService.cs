using EstateHub.ListingService.Domain.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EstateHub.ListingService.Infrastructure.Services;

public class SmtpOptions
{
    public const string Smtp = "Smtp";

    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
}

public class ListingNotificationService : IListingNotificationService
{
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<ListingNotificationService> _logger;

    public ListingNotificationService(
        IOptions<SmtpOptions> smtpOptions,
        ILogger<ListingNotificationService> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendListingUnpublishedNotificationAsync(
        string userEmail,
        string listingTitle,
        Guid listingId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emailMessage = new MimeMessage();

            var fromAddress = string.IsNullOrWhiteSpace(_smtpOptions.User) 
                ? "noreply@estatehub.local" 
                : _smtpOptions.User;
            
            emailMessage.From.Add(new MailboxAddress("EstateHub", fromAddress));
            emailMessage.To.Add(new MailboxAddress("", userEmail));
            emailMessage.Subject = "Your Listing Has Been Unpublished";

            var escapedTitle = System.Net.WebUtility.HtmlEncode(listingTitle);
            var escapedReason = System.Net.WebUtility.HtmlEncode(reason);

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <h2 style='color: #d32f2f;'>Listing Unpublished</h2>
                            <p>Hello,</p>
                            <p>We wanted to inform you that your listing <strong>&quot;{escapedTitle}&quot;</strong> has been unpublished by an administrator.</p>
                            <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #856404;'>Reason:</h3>
                                <p style='margin-bottom: 0; white-space: pre-wrap;'>{escapedReason}</p>
                            </div>
                            <p>If you believe this action was taken in error, please contact our support team for assistance.</p>
                            <p>You can edit your listing and resubmit it for review once you've addressed the concerns mentioned above.</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                            <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
                        </div>
                    </body>
                    </html>"
            };

            emailMessage.Body = bodyBuilder.ToMessageBody();

            await SendMessageAsync(
                _smtpOptions.User,
                _smtpOptions.Password,
                _smtpOptions.Host,
                _smtpOptions.Port,
                emailMessage,
                cancellationToken);

            _logger.LogInformation(
                "Listing unpublished notification sent - ListingId: {ListingId}, Email: {Email}",
                listingId,
                userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send listing unpublished notification - ListingId: {ListingId}, Email: {Email}",
                listingId,
                userEmail);
            throw;
        }
    }

    private static async Task SendMessageAsync(
        string user,
        string password,
        string smtpHost,
        int smtpPort,
        MimeMessage email,
        CancellationToken cancellationToken = default)
    {
        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(smtpHost, smtpPort, false, cancellationToken);

        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password))
        {
            await smtp.AuthenticateAsync(user, password, cancellationToken);
        }

        await smtp.SendAsync(email, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}

