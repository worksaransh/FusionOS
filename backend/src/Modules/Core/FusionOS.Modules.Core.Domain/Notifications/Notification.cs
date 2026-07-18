using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Notifications;

public sealed class Notification : TenantAggregateRoot
{
    public Guid RecipientUserId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public bool IsRead { get; private set; }
    public NotificationDeliveryStatus DeliveryStatus { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public string? DeliveryError { get; private set; }

    private Notification() { }

    public static Notification Create(Guid companyId, Guid recipientUserId, string title, string body)
    {
        var notification = new Notification
        {
            CompanyId = companyId,
            RecipientUserId = recipientUserId,
            Title = title,
            Body = body,
            DeliveryStatus = NotificationDeliveryStatus.Pending,
        };
        return notification;
    }

    public void MarkRead() => IsRead = true;

    /// <summary>Recorded by NotificationDeliveryDispatcher after a successful SendGrid send.</summary>
    public void MarkDelivered()
    {
        DeliveryStatus = NotificationDeliveryStatus.Sent;
        DeliveredAt = DateTimeOffset.UtcNow;
        DeliveryError = null;
    }

    /// <summary>
    /// Recorded after a failed SendGrid send. Deliberately not terminal — stays
    /// eligible for retry on the dispatcher's next poll (see
    /// NotificationDeliveryStatus's doc comment).
    /// </summary>
    public void MarkDeliveryFailed(string error)
    {
        DeliveryStatus = NotificationDeliveryStatus.Failed;
        DeliveryError = error;
    }
}
