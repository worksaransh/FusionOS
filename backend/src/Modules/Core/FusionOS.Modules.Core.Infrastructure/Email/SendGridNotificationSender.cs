using FusionOS.Modules.Core.Application.Notifications.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FusionOS.Modules.Core.Infrastructure.Email;

/// <summary>
/// The only place in this codebase that references the SendGrid SDK (Phase M7
/// remaining, 2026-07-16 — the notification-provider decision resolved to
/// SendGrid; NotificationDeliveryService and its tests depend only on the
/// provider-agnostic INotificationSender). Body is sent as plain text — the
/// existing Notification.Body is already plain text (see the Approvals
/// workflow's notification copy), so no HTML templating is introduced here.
/// </summary>
public sealed class SendGridNotificationSender : INotificationSender
{
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridNotificationSender> _logger;

    public SendGridNotificationSender(IOptions<SendGridOptions> options, ILogger<SendGridNotificationSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            // Deliberately throws rather than silently no-op-ing: the caller
            // (NotificationDeliveryService) catches this and records a Failed
            // delivery with this exact message, so an unconfigured API key is
            // visible in the notification list/DeliveryError rather than
            // silently swallowed — same "make the gap visible, don't fake
            // success" restraint used throughout this codebase's placeholder
            // integrations.
            throw new InvalidOperationException("SendGrid is not configured (SendGrid:ApiKey is blank).");
        }

        var client = new SendGridClient(_options.ApiKey);
        var from = new EmailAddress(_options.FromAddress, _options.FromName);
        var to = new EmailAddress(toEmail);
        var message = MailHelper.CreateSingleEmail(from, to, subject, body, htmlContent: null);

        var response = await client.SendEmailAsync(message, cancellationToken);

        if ((int)response.StatusCode is < 200 or >= 300)
        {
            var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("SendGrid returned {StatusCode} sending to {Recipient}: {Body}", response.StatusCode, toEmail, responseBody);
            throw new InvalidOperationException($"SendGrid returned {(int)response.StatusCode} sending to {toEmail}.");
        }
    }
}
