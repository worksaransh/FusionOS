using FusionOS.Modules.Core.Application.Notifications.Commands.MarkNotificationRead;
using FusionOS.Modules.Core.Application.Notifications.Queries.ListNotifications;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// In-app notification read side (Phase M7, 2026-07-15) — makes the
/// previously fully-dormant Notification entity usable. External delivery
/// (email via SendGrid, Phase M7 remaining, 2026-07-16) now runs as a
/// background dispatcher (NotificationDeliveryDispatcher) rather than a
/// request-driven endpoint, so there is no delivery action on this
/// controller — DeliveryStatus surfaces read-only on NotificationDto.
/// </summary>
[ApiController]
[Route("api/v1/core/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] bool unreadOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListNotificationsQuery(companyId, unreadOnly, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/mark-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MarkNotificationReadCommand(companyId, id), cancellationToken);
        return Ok(result);
    }
}
