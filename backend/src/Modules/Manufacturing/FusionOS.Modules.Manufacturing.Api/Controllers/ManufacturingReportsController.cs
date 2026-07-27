using FusionOS.Modules.Manufacturing.Application.WorkOrders.Queries.GetWorkInProgressSummaryReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Manufacturing.Api.Controllers;

/// <summary>Canned reports over Manufacturing data (WIP support, Phase 6, 2026-07-22) — same pattern as Finance's ReportsController.</summary>
[ApiController]
[Route("api/v1/manufacturing/reports")]
public sealed class ManufacturingReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ManufacturingReportsController(ISender sender) => _sender = sender;

    /// <summary>Every Released work order's currently-issued material value, and the company-wide WIP total. See GetWorkInProgressSummaryReportQuery's doc comment for the valuation basis and its limitations.</summary>
    [HttpGet("work-in-progress-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkInProgressSummary([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWorkInProgressSummaryReportQuery(companyId), cancellationToken);
        return Ok(result);
    }
}
