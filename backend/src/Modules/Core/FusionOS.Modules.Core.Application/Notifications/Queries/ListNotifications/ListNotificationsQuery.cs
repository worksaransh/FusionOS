using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Notifications.Contracts;

namespace FusionOS.Modules.Core.Application.Notifications.Queries.ListNotifications;

/// <summary>Always the calling user's own notifications — RecipientUserId comes from ICurrentUserContext, never a client-supplied id, so one user can never read another's notifications.</summary>
public sealed record ListNotificationsQuery(Guid CompanyId, bool UnreadOnly, int Page, int PageSize) : IQuery<PagedResult<NotificationDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.notification.read" };
}
