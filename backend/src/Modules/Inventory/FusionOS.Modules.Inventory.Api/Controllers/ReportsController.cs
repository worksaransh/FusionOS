using FusionOS.Modules.Inventory.Application.Reports.Queries.GetInventoryValuationReport;
using FusionOS.Modules.Inventory.Application.Reports.Queries.GetStockValuationReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Inventory.Api.Controllers;

/// <summary>Canned reports over Inventory data (Phase M6, 2026-07-15; costing report added M9 remaining, 2026-07-16).</summary>
[ApiController]
[Route("api/v1/inventory/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender) => _sender = sender;

    [HttpGet("stock-valuation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockValuation([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetStockValuationReportQuery(companyId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Weighted-average-cost valuation + cumulative COGS per product (M9 remaining, 2026-07-16).</summary>
    [HttpGet("inventory-valuation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryValuation([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInventoryValuationReportQuery(companyId), cancellationToken);
        return Ok(result);
    }
}
