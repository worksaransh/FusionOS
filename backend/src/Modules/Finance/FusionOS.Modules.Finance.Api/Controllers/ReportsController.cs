using FusionOS.Modules.Finance.Application.Reports.Queries.GetApAgingReport;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetArAgingReport;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetBalanceSheetReport;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetCashFlowStatementReport;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetProfitAndLossReport;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetTaxPeriodSummaryReport;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetTrialBalance;
using FusionOS.Modules.Finance.Application.TaxTransactions.Queries.GetGstReturnSummaryReport;
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

    [HttpGet("cash-flow-statement")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCashFlowStatement([FromQuery] Guid companyId, [FromQuery] DateTimeOffset periodStart, [FromQuery] DateTimeOffset periodEnd, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCashFlowStatementReportQuery(companyId, periodStart, periodEnd), cancellationToken);
        return Ok(result);
    }

    [HttpGet("tax-period-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxPeriodSummary([FromQuery] Guid companyId, [FromQuery] DateTimeOffset periodStart, [FromQuery] DateTimeOffset periodEnd, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTaxPeriodSummaryReportQuery(companyId, periodStart, periodEnd), cancellationToken);
        return Ok(result);
    }

    // GST filing support (2026-07-22) — unlike tax-period-summary above (a rate-register
    // scaffold with null amounts), this reads real TaxTransaction rows once Sales/Procurement's
    // event extensions populate them.
    [HttpGet("gst-return-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGstReturnSummary([FromQuery] Guid companyId, [FromQuery] DateTimeOffset periodStart, [FromQuery] DateTimeOffset periodEnd, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetGstReturnSummaryReportQuery(companyId, periodStart, periodEnd), cancellationToken);
        return Ok(result);
    }
}
