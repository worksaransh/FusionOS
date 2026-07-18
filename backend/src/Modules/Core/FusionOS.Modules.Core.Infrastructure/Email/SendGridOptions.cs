namespace FusionOS.Modules.Core.Infrastructure.Email;

/// <summary>
/// Bound from the "SendGrid" configuration section — see appsettings.json and
/// 07_SECURITY.md's secrets-out-of-source-control pattern (same shape as
/// JwtOptions). Unlike Jwt:SigningKey and ConnectionStrings:Postgres, a blank
/// ApiKey does NOT fail the app at startup: notification delivery degrades
/// gracefully (SendGridNotificationSender throws per-send, which
/// NotificationDeliveryService catches and records as a Failed delivery,
/// retried on the next poll) rather than the whole API refusing to boot over
/// what is, today, a best-effort side channel — in-app notifications
/// (NotificationsPage) keep working regardless.
/// </summary>
public sealed class SendGridOptions
{
    public const string SectionName = "SendGrid";

    public string ApiKey { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "notifications@fusionos.local";
    public string FromName { get; set; } = "FusionOS";
}
