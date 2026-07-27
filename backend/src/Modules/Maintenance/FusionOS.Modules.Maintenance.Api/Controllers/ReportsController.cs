using FusionOS.Modules.Maintenance.Application.Reports.Queries.GetDowntimeMetricsReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Maintenance.Api.Controllers;

/// <summary>Canned reports over Maintenance data — mirrors Finance's ReportsController shape (api/v1/finance/reports).</summary>
[ApiController]
[Route("api/v1/maintenance/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender) => _sender = sender;

    [HttpGet("downtime-metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDowntimeMetrics([FromQuery] Guid companyId, [FromQuery] Guid? assetId, [FromQuery] DateTimeOffset periodStart, [FromQuery] DateTimeOffset periodEnd, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDowntimeMetricsReportQuery(companyId, assetId, periodStart, periodEnd), cancellationToken);
        return Ok(result);
    }
}
