namespace FusionOS.Modules.Core.Application.Notifications.Contracts;

public interface INotificationRepository
{
    Task AddAsync(Domain.Notifications.Notification notification, CancellationToken cancellationToken = default);

    Task<Domain.Notifications.Notification?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Notifications.Notification>> ListAsync(Guid companyId, Guid recipientUserId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, Guid recipientUserId, bool unreadOnly, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every notification not yet Sent (Pending or Failed — Failed is retried,
    /// see NotificationDeliveryStatus), across every company. Deliberately not
    /// tenant-scoped: NotificationDeliveryDispatcher is a system-wide background
    /// job, same "no CompanyId filter" precedent as OutboxDispatcher's own query.
    /// </summary>
    Task<IReadOnlyList<Domain.Notifications.Notification>> GetPendingDeliveryAsync(int batchSize, CancellationToken cancellationToken = default);
}
