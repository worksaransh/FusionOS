namespace FusionOS.Modules.Core.Application.Notifications.Contracts;

/// <summary>
/// Abstraction over the external email provider (Phase M7 remaining, 2026-07-16
/// — resolved to SendGrid). Kept provider-agnostic at the Application layer so
/// NotificationDeliveryService and its tests never reference the SendGrid SDK
/// directly — only FusionOS.Modules.Core.Infrastructure's SendGridNotificationSender
/// does. Throws on any non-success response; callers (NotificationDeliveryService)
/// catch and record the failure via Notification.MarkDeliveryFailed rather than
/// letting a single bad send take down the whole poll batch.
/// </summary>
public interface INotificationSender
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}
