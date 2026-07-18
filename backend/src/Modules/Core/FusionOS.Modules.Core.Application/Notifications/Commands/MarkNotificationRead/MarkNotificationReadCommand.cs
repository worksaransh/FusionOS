using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Notifications.Contracts;

namespace FusionOS.Modules.Core.Application.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid CompanyId, Guid NotificationId) : ICommand<NotificationDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.notification.read" };
}
