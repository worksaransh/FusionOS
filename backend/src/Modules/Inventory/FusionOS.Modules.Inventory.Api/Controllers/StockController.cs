using FusionOS.Modules.Inventory.Application.Ledger.Commands.AdjustStock;
using FusionOS.Modules.Inventory.Application.Ledger.Queries.GetStockOnHand;
using FusionOS.Modules.Inventory.Application.Ledger.Queries.ListLedgerEntries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Inventory.Api.Controllers;

/// <summary>Inventory Ledger / stock adjustments — next slice after Product (04_DATABASE_GUIDELINES.md §12, 08_API_STANDARDS.md).</summary>
[ApiController]
[Route("api/v1/inventory/stock")]
public sealed class StockController : ControllerBase
{
    private readonly ISender _sender;

    public StockController(ISender sender) => _sender = sender;

    [HttpPost("adjustments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Adjust([FromBody] AdjustStockCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetOnHand), new { companyId = command.CompanyId, productId = command.ProductId, warehouseId = command.WarehouseId }, result);
    }

    [HttpGet("on-hand")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOnHand([FromQuery] Guid companyId, [FromQuery] Guid productId, [FromQuery] Guid? warehouseId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockOnHandQuery(companyId, productId, warehouseId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("ledger")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLedger([FromQuery] Guid companyId, [FromQuery] Guid productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListLedgerEntriesQuery(companyId, productId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
