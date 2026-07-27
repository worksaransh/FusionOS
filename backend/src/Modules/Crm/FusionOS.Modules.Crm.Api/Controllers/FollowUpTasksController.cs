using FusionOS.Modules.Crm.Application.Tasks.Commands.CompleteFollowUpTask;
using FusionOS.Modules.Crm.Application.Tasks.Commands.CreateFollowUpTask;
using FusionOS.Modules.Crm.Application.Tasks.Queries.GetFollowUpTaskById;
using FusionOS.Modules.Crm.Application.Tasks.Queries.ListFollowUpTasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Crm.Api.Controllers;

/// <summary>
/// Phase 4 — CRM: follow-up tasks/reminders. A small open→Completed to-do against a
/// Lead, Opportunity, Account, or Contact — same opaque (EntityType, EntityId)
/// polymorphic reference as ActivitiesController (see FollowUpTask.cs). Route matches
/// the already-reserved crm.task.* permission family in PermissionCatalog.
/// </summary>
[ApiController]
[Route("api/v1/crm/tasks")]
public sealed class FollowUpTasksController : ControllerBase
{
    private readonly ISender _sender;

    public FollowUpTasksController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateFollowUpTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateFollowUpTaskCommand(
            request.CompanyId, request.Title, request.Description, request.DueDate,
            request.EntityType, request.EntityId, request.AssignedToUserId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetFollowUpTaskByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] bool? isCompleted = null,
        [FromQuery] bool overdueOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListFollowUpTasksQuery(companyId, assignedToUserId, entityType, entityId, isCompleted, overdueOnly, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] FollowUpTaskActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompleteFollowUpTaskCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateFollowUpTaskRequest(
    Guid CompanyId,
    string Title,
    string? Description,
    DateTimeOffset DueDate,
    string EntityType,
    Guid EntityId,
    Guid? AssignedToUserId);

public sealed record FollowUpTaskActionRequest(Guid CompanyId);
