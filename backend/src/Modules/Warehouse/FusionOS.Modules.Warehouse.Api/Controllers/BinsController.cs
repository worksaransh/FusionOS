using FusionOS.Modules.Warehouse.Application.Bins.Commands.AssignBinShelf;
using FusionOS.Modules.Warehouse.Application.Bins.Commands.CreateBin;
using FusionOS.Modules.Warehouse.Application.Bins.Commands.DeactivateBin;
using FusionOS.Modules.Warehouse.Application.Bins.Commands.UpdateBin;
using FusionOS.Modules.Warehouse.Application.Bins.Queries.GetBinById;
using FusionOS.Modules.Warehouse.Application.Bins.Queries.ListBins;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Warehouse.Api.Controllers;

/// <summary>Bins — next slice after Zone (docs/IMPLEMENTATION_PLAN.md Phase 9 "bins" item). Nested under warehouses/zones, one level deeper than ZonesController.</summary>
[ApiController]
[Route("api/v1/warehouse/warehouses/{warehouseId:guid}/zones/{zoneId:guid}/bins")]
public sealed class BinsController : ControllerBase
{
    private readonly ISender _sender;

    public BinsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(Guid warehouseId, Guid zoneId, [FromBody] CreateBinRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateBinCommand(request.CompanyId, zoneId, request.Name, request.Code);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { warehouseId, zoneId, id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid warehouseId, Guid zoneId, Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBinByIdQuery(companyId, id), cancellationToken);
        if (result is null || result.ZoneId != zoneId)
            return NotFound();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid warehouseId, Guid zoneId, [FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListBinsQuery(companyId, zoneId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid warehouseId, Guid zoneId, Guid id, [FromBody] UpdateBinRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBinCommand(request.CompanyId, id, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a DELETE, this never removes the row.
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(Guid warehouseId, Guid zoneId, Guid id, [FromBody] DeactivateBinRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateBinCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    // Assigns (or clears, when request.ShelfId is null) this bin's optional Shelf
    // refinement — reuses "warehouse.bin.update" (see AssignBinShelfCommand), not
    // a new permission-worthy action. AssignBinShelfCommandHandler rejects the
    // assignment if the shelf's rack's zone doesn't match this bin's own zoneId.
    [HttpPost("{id:guid}/shelf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignShelf(Guid warehouseId, Guid zoneId, Guid id, [FromBody] AssignBinShelfRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AssignBinShelfCommand(request.CompanyId, id, request.ShelfId), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateBinRequest(Guid CompanyId, string Name, string Code);
public sealed record UpdateBinRequest(Guid CompanyId, string Name);
public sealed record DeactivateBinRequest(Guid CompanyId);
public sealed record AssignBinShelfRequest(Guid CompanyId, Guid? ShelfId);
