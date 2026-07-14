using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.CreateGoodsReceipt;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Queries.ListGoodsReceipts;
using FusionOS.Modules.Warehouse.Domain.GoodsReceipts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Warehouse.Api.Controllers;

/// <summary>
/// Goods Receipts — next slice after Zones (03_SYSTEM_ARCHITECTURE.md §4.2 event
/// catalog: "GoodsReceived.v1", produced by Warehouse). Nested under warehouses
/// per 08_API_STANDARDS.md §3, same as Zones.
/// </summary>
[ApiController]
[Route("api/v1/warehouse/warehouses/{warehouseId:guid}/goods-receipts")]
public sealed class GoodsReceiptsController : ControllerBase
{
    private readonly ISender _sender;

    public GoodsReceiptsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(Guid warehouseId, [FromBody] CreateGoodsReceiptRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new GoodsReceiptLineInput(l.ProductId, l.QuantityReceived, l.UnitCost)).ToList();
        var command = new CreateGoodsReceiptCommand(request.CompanyId, warehouseId, request.ZoneId, request.PurchaseOrderId, request.SupplierId, lines);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { warehouseId, companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid warehouseId, [FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListGoodsReceiptsQuery(companyId, warehouseId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

/// <summary>Request body for goods receipt creation — WarehouseId comes from the route, not the body, per REST convention.</summary>
public sealed record CreateGoodsReceiptRequest(Guid CompanyId, Guid ZoneId, Guid? PurchaseOrderId, Guid? SupplierId, IReadOnlyList<CreateGoodsReceiptLineRequest> Lines);

public sealed record CreateGoodsReceiptLineRequest(Guid ProductId, decimal QuantityReceived, decimal? UnitCost);
