using FusionOS.Modules.Warehouse.Application.PickLists.Commands.AssignPickList;
using FusionOS.Modules.Warehouse.Application.PickLists.Commands.CreatePickList;
using FusionOS.Modules.Warehouse.Application.PickLists.Commands.PackPickList;
using FusionOS.Modules.Warehouse.Application.PickLists.Commands.RecordPick;
using FusionOS.Modules.Warehouse.Application.PickLists.Queries.GetPickListById;
using FusionOS.Modules.Warehouse.Application.PickLists.Queries.ListPickLists;
using FusionOS.Modules.Warehouse.Domain.PickLists;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Warehouse.Api.Controllers;

/// <summary>
/// Picking + Packing (docs/IMPLEMENTATION_PLAN.md Phase 9 items 10-11). Nested under warehouses like
/// GoodsReceipts/CycleCounts. A pick list is created against a (opaque, not validated) Sales Order
/// id, then Assign / Record / Pack advance it through Pending -> Assigned -> Picked -> Packed.
/// </summary>
[ApiController]
[Route("api/v1/warehouse/warehouses/{warehouseId:guid}/pick-lists")]
public sealed class PickListsController : ControllerBase
{
    private readonly ISender _sender;

    public PickListsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(Guid warehouseId, [FromBody] CreatePickListRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new PickListLineInput(l.ProductId, l.BinId, l.QuantityToPick)).ToList();
        var command = new CreatePickListCommand(request.CompanyId, warehouseId, request.SalesOrderId, lines);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { warehouseId, id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid warehouseId, Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPickListByIdQuery(companyId, id), cancellationToken);
        if (result is null || result.WarehouseId != warehouseId)
            return NotFound();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid warehouseId, [FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListPickListsQuery(companyId, warehouseId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(Guid warehouseId, Guid id, [FromBody] AssignPickListRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AssignPickListCommand(request.CompanyId, id, request.AssignedToUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/record")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Record(Guid warehouseId, Guid id, [FromBody] RecordPickRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RecordPickCommand(request.CompanyId, id, request.LineId, request.QuantityPicked), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/pack")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pack(Guid warehouseId, Guid id, [FromBody] PackPickListRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PackPickListCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreatePickListLineRequest(Guid ProductId, Guid? BinId, decimal QuantityToPick);
public sealed record CreatePickListRequest(Guid CompanyId, Guid SalesOrderId, IReadOnlyList<CreatePickListLineRequest> Lines);
public sealed record AssignPickListRequest(Guid CompanyId, Guid AssignedToUserId);
public sealed record RecordPickRequest(Guid CompanyId, Guid LineId, decimal QuantityPicked);
public sealed record PackPickListRequest(Guid CompanyId);
