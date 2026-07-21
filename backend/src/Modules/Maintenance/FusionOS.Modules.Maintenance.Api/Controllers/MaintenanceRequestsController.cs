using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.AssignMaintenanceRequestTechnician;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CompleteMaintenanceRequest;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CreateMaintenanceRequest;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.StartMaintenanceRequest;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Queries.GetMaintenanceRequestById;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Queries.ListMaintenanceRequests;
using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Maintenance.Api.Controllers;

/// <summary>
/// Phase 5 — Maintenance: preventive/breakdown maintenance requests against an Asset,
/// Open → InProgress → Completed. Completed requests against one Asset are this
/// module's "maintenance history" (see AssetId's optional filter on List below).
/// </summary>
[ApiController]
[Route("api/v1/maintenance/requests")]
public sealed class MaintenanceRequestsController : ControllerBase
{
    private readonly ISender _sender;

    public MaintenanceRequestsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequestRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateMaintenanceRequestCommand(request.CompanyId, request.AssetId, request.Type, request.Description);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMaintenanceRequestByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? assetId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListMaintenanceRequestsQuery(companyId, assetId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start(Guid id, [FromBody] MaintenanceRequestActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new StartMaintenanceRequestCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteMaintenanceRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompleteMaintenanceRequestCommand(request.CompanyId, id, request.ResolutionNotes, request.ActualDowntimeMinutes), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/assign-technician")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTechnician(Guid id, [FromBody] AssignMaintenanceRequestTechnicianRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AssignMaintenanceRequestTechnicianCommand(request.CompanyId, id, request.TechnicianUserId), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateMaintenanceRequestRequest(Guid CompanyId, Guid AssetId, MaintenanceRequestType Type, string Description);

public sealed record MaintenanceRequestActionRequest(Guid CompanyId);

public sealed record CompleteMaintenanceRequestRequest(Guid CompanyId, string? ResolutionNotes, int? ActualDowntimeMinutes = null);

public sealed record AssignMaintenanceRequestTechnicianRequest(Guid CompanyId, Guid TechnicianUserId);
