using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.CreateMaintenanceSchedule;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.DeactivateMaintenanceSchedule;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.UpdateMaintenanceSchedule;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Queries.GetMaintenanceScheduleById;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Queries.ListMaintenanceSchedules;
using FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Maintenance.Api.Controllers;

/// <summary>
/// Phase 5 — Maintenance: preventive maintenance schedules against an Asset — a
/// recurrence plan ("every Quarter") with a NextDueDate, distinct from
/// MaintenanceRequest (a single already-reported unit of work). CRUD-ish shape
/// (create/read/list/update/soft-deactivate) mirroring AssetsController, plus a
/// due-date filter on List for the "due soon"/"overdue" views.
/// </summary>
[ApiController]
[Route("api/v1/maintenance/schedules")]
public sealed class MaintenanceSchedulesController : ControllerBase
{
    private readonly ISender _sender;

    public MaintenanceSchedulesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceScheduleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateMaintenanceScheduleCommand(request.CompanyId, request.AssetId, request.Frequency, request.Description, request.NextDueDate);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMaintenanceScheduleByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? assetId = null,
        [FromQuery] MaintenanceScheduleDueFilter? dueFilter = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListMaintenanceSchedulesQuery(companyId, assetId, dueFilter, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMaintenanceScheduleRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateMaintenanceScheduleCommand(request.CompanyId, id, request.Frequency, request.Description, request.NextDueDate);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateMaintenanceScheduleRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateMaintenanceScheduleCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateMaintenanceScheduleRequest(Guid CompanyId, Guid AssetId, MaintenanceScheduleFrequency Frequency, string Description, DateTimeOffset NextDueDate);

public sealed record UpdateMaintenanceScheduleRequest(Guid CompanyId, MaintenanceScheduleFrequency Frequency, string Description, DateTimeOffset NextDueDate);

public sealed record DeactivateMaintenanceScheduleRequest(Guid CompanyId);
