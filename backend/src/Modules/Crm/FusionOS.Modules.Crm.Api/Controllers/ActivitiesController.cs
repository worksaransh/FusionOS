using FusionOS.Modules.Crm.Application.Activities.Commands.CreateActivity;
using FusionOS.Modules.Crm.Application.Activities.Queries.GetActivityById;
using FusionOS.Modules.Crm.Application.Activities.Queries.ListActivities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Crm.Api.Controllers;

/// <summary>
/// Phase 4 — CRM: Activities. A logged interaction (call/email/meeting/note) against a
/// Lead, Opportunity, Account, or Contact — same opaque (EntityType, EntityId)
/// polymorphic reference as Core's ApprovalRequest (see Activity.cs). A point-in-time
/// log entry, not a lifecycle aggregate: create + read only, no update/deactivate.
/// </summary>
[ApiController]
[Route("api/v1/crm/activities")]
public sealed class ActivitiesController : ControllerBase
{
    private readonly ISender _sender;

    public ActivitiesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateActivityRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateActivityCommand(request.CompanyId, request.EntityType, request.EntityId, request.Type, request.Notes);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetActivityByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid companyId,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListActivitiesQuery(companyId, entityType, entityId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateActivityRequest(Guid CompanyId, string EntityType, Guid EntityId, string Type, string Notes);
