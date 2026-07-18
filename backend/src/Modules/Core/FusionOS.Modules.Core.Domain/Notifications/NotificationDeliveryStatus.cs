namespace FusionOS.Modules.Core.Domain.Notifications;

/// <summary>
/// External-delivery status (Phase M7 remaining, 2026-07-16 — the notification-
/// provider decision resolved to SendGrid). Failed is deliberately NOT terminal:
/// NotificationDeliveryDispatcher retries anything not yet Sent on its next poll,
/// same at-least-once philosophy as OutboxDispatcher's ProcessedOn == null retry.
/// This is separate from IsRead, which tracks whether the in-app notification
/// itself has been viewed — a notification can be Sent (emailed) and still
/// unread, or Failed (email bounced) and already read in-app.
/// </summary>
public enum NotificationDeliveryStatus
{
    Pending,
    Sent,
    Failed,
}
