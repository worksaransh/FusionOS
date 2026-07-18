using FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.RecordCycleCount;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.StartCycleCount;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Queries.GetCycleCountById;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Queries.ListCycleCounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Warehouse.Api.Controllers;

/// <summary>
/// Cycle counting (docs/IMPLEMENTATION_PLAN.md Phase 9 — "Cycle counting
/// (warehouse side)"). Nested under warehouses like GoodsReceipts, since a
/// count belongs to one warehouse even though it references a Zone/Bin
/// within it. Two-step lifecycle: POST starts a count (system quantity
/// snapshot supplied by the caller), POST /{id}/record submits what was
/// physically counted.
/// </summary>
[ApiController]
[Route("api/v1/warehouse/warehouses/{warehouseId:guid}/cycle-counts")]
public sealed class CycleCountsController : ControllerBase
{
    private readonly ISender _sender;

    public CycleCountsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Start(Guid warehouseId, [FromBody] StartCycleCountRequest request, CancellationToken cancellationToken)
    {
        var command = new StartCycleCountCommand(request.CompanyId, warehouseId, request.ZoneId, request.BinId, request.ProductId, request.SystemQuantitySnapshot);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { warehouseId, id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid warehouseId, Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCycleCountByIdQuery(companyId, id), cancellationToken);
        if (result is null || result.WarehouseId != warehouseId)
            return NotFound();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid warehouseId, [FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCycleCountsQuery(companyId, warehouseId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/record")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Record(Guid warehouseId, Guid id, [FromBody] RecordCycleCountRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RecordCycleCountCommand(request.CompanyId, id, request.CountedQuantity), cancellationToken);
        return Ok(result);
    }
}

public sealed record StartCycleCountRequest(Guid CompanyId, Guid ZoneId, Guid BinId, Guid ProductId, decimal SystemQuantitySnapshot);
public sealed record RecordCycleCountRequest(Guid CompanyId, decimal CountedQuantity);
