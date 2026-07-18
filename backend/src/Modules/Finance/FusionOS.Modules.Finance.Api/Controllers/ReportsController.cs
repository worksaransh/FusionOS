using FusionOS.Modules.Finance.Application.Reports.Queries.GetApAgingReport;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetArAgingReport;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetBalanceSheetReport;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetProfitAndLossReport;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetTrialBalance;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>Canned reports over Finance data (Phase M6, 2026-07-15).</summary>
[ApiController]
[Route("api/v1/finance/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender) => _sender = sender;

    [HttpGet("ar-aging")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetArAging([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetArAgingReportQuery(companyId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("trial-balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrialBalance([FromQuery] Guid companyId, [FromQuery] DateTimeOffset asOfDate, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTrialBalanceQuery(companyId, asOfDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("ap-aging")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApAging([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetApAgingReportQuery(companyId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("profit-and-loss")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfitAndLoss([FromQuery] Guid companyId, [FromQuery] DateTimeOffset periodStart, [FromQuery] DateTimeOffset periodEnd, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProfitAndLossReportQuery(companyId, periodStart, periodEnd), cancellationToken);
        return Ok(result);
    }

    [HttpGet("balance-sheet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] Guid companyId, [FromQuery] DateTimeOffset asOfDate, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBalanceSheetReportQuery(companyId, asOfDate), cancellationToken);
        return Ok(result);
    }
}
