using FusionOS.Modules.Sales.Application.Commissions.Commands.SetCommissionRate;
using FusionOS.Modules.Sales.Application.Commissions.Queries.GetSalesCommissionSummaryReport;
using FusionOS.Modules.Sales.Application.Commissions.Queries.ListCommissionRates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>Per-salesperson commission rates and the invoiced-revenue commission summary report (docs/IMPLEMENTATION_PLAN.md Phase 10 item 11). Commission is computed on invoiced, not just ordered, revenue.</summary>
[ApiController]
[Route("api/v1/sales/commissions")]
public sealed class CommissionsController : ControllerBase
{
    private readonly ISender _sender;

    public CommissionsController(ISender sender) => _sender = sender;

    [HttpPost("rates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetRate([FromBody] SetCommissionRateCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("rates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRates([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCommissionRatesQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("summary-report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummaryReport([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSalesCommissionSummaryReportQuery(companyId), cancellationToken);
        return Ok(result);
    }
}
