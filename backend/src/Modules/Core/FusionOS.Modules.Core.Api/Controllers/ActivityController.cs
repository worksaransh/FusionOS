using FusionOS.Modules.Core.Application.Activity.Queries.GetEntityActivityTimeline;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// Read-only, merged activity feed for one (EntityType, EntityId) — combines
/// AuditLogController's system-generated trail with CommentsController's
/// user-authored notes into one chronological list. A new controller rather
/// than folding this onto AuditLogController or CommentsController: it isn't
/// "more audit log" or "more comments," it's a third, composed read model
/// with its own permission ("core.activity.read") that doesn't imply either
/// "core.audit.read" or "core.comment.read" on its own.
/// </summary>
[ApiController]
[Route("api/v1/core/activity")]
public sealed class ActivityController : ControllerBase
{
    private readonly ISender _sender;

    public ActivityController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] Guid companyId, [FromQuery] string entityType, [FromQuery] Guid entityId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetEntityActivityTimelineQuery(companyId, entityType, entityId), cancellationToken);
        return Ok(result);
    }
}
