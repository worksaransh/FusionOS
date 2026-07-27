using FusionOS.Modules.Manufacturing.Application.WorkCenters.Commands.CreateWorkCenter;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Commands.DeactivateWorkCenter;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Queries.GetWorkCenterCapacityLoad;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Queries.ListWorkCenters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Manufacturing.Api.Controllers;

/// <summary>
/// Capacity Planning support (Phase 6, 2026-07-22): WorkCenters. Master data describing
/// a shop-floor work center's daily capacity — CRUD-ish shape (create/read/list/soft-
/// deactivate) mirroring BillsOfMaterialsController, plus a capacity-load endpoint
/// comparing available vs. scheduled hours over a period.
/// </summary>
[ApiController]
[Route("api/v1/manufacturing/work-centers")]
public sealed class WorkCentersController : ControllerBase
{
    private readonly ISender _sender;

    public WorkCentersController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateWorkCenterRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateWorkCenterCommand(request.CompanyId, request.Code, request.Name, request.CapacityHoursPerDay);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListWorkCentersQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateWorkCenterRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateWorkCenterCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Capacity Planning: available (CapacityHoursPerDay × days in period) vs. required (scheduled operation hours matched by work-center name/code) hours over a period.</summary>
    [HttpGet("{id:guid}/capacity-load")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCapacityLoad(
        Guid id,
        [FromQuery] Guid companyId,
        [FromQuery] DateTimeOffset periodStart,
        [FromQuery] DateTimeOffset periodEnd,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWorkCenterCapacityLoadQuery(companyId, id, periodStart, periodEnd), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateWorkCenterRequest(Guid CompanyId, string Code, string Name, decimal CapacityHoursPerDay);

public sealed record DeactivateWorkCenterRequest(Guid CompanyId);
