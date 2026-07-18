using FusionOS.Modules.Procurement.Application.Reports.Queries.GetPoStatusSummaryReport;
using FusionOS.Modules.Procurement.Application.Reports.Queries.GetPriceHistoryReport;
using FusionOS.Modules.Procurement.Application.Reports.Queries.GetSupplierScorecardReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Procurement.Api.Controllers;

/// <summary>Canned reports over Procurement data (Phase M6, 2026-07-15).</summary>
[ApiController]
[Route("api/v1/procurement/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender) => _sender = sender;

    [HttpGet("po-status-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPoStatusSummary([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPoStatusSummaryReportQuery(companyId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("supplier-scorecard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupplierScorecard([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSupplierScorecardReportQuery(companyId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("price-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPriceHistory([FromQuery] Guid companyId, [FromQuery] Guid productId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPriceHistoryReportQuery(companyId, productId), cancellationToken);
        return Ok(result);
    }
}
