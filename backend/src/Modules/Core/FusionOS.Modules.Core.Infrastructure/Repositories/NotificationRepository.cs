using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.Modules.Core.Domain.Notifications;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly CoreDbContext _context;

    public NotificationRepository(CoreDbContext context) => _context = context;

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default) =>
        await _context.Notifications.AddAsync(notification, cancellationToken);

    public Task<Notification?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.Notifications.FirstOrDefaultAsync(n => n.CompanyId == companyId && n.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Notification>> ListAsync(Guid companyId, Guid recipientUserId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, recipientUserId, unreadOnly)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid recipientUserId, bool unreadOnly, CancellationToken cancellationToken = default) =>
        Filtered(companyId, recipientUserId, unreadOnly).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetPendingDeliveryAsync(int batchSize, CancellationToken cancellationToken = default) =>
        await _context.Notifications
            .Where(n => n.DeliveryStatus == NotificationDeliveryStatus.Pending || n.DeliveryStatus == NotificationDeliveryStatus.Failed)
            .OrderBy(n => n.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

    private IQueryable<Notification> Filtered(Guid companyId, Guid recipientUserId, bool unreadOnly)
    {
        var query = _context.Notifications.Where(n => n.CompanyId == companyId && n.RecipientUserId == recipientUserId);
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);
        return query;
    }
}
