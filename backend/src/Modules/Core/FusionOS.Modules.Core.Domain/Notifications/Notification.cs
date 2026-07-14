using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Notifications;

public sealed class Notification : TenantAggregateRoot
{
    public Guid RecipientUserId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public bool IsRead { get; private set; }

    private Notification() { }

    public static Notification Create(Guid companyId, Guid recipientUserId, string title, string body)
    {
        var notification = new Notification
        {
            CompanyId = companyId,
            RecipientUserId = recipientUserId,
            Title = title,
            Body = body,
        };
        return notification;
    }

    public void MarkRead() => IsRead = true;
}
