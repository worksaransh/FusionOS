using FusionOS.Modules.Ai.Application.Reports.Contracts;
using FusionOS.Modules.Ai.Application.Reports.Queries.GenerateKpiNarrativeReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Ai.Api.Controllers;

/// <summary>
/// Phase 7/8 — AI Platform: automated narrative report generation, building on
/// Business Intelligence's own KPI data rather than a parallel reporting
/// pipeline (12_AI_PLATFORM.md §4) — see
/// <see cref="GenerateKpiNarrativeReportQueryHandler"/> for how BI's data is
/// read exclusively through BI's own MediatR queries.
/// </summary>
[ApiController]
[Route("api/v1/ai/reports")]
public sealed class AiReportsController : ControllerBase
{
    private readonly ISender _sender;

    public AiReportsController(ISender sender) => _sender = sender;

    [HttpGet("kpi-narrative/{kpiDefinitionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KpiNarrativeReportDto>> GetKpiNarrative(
        Guid kpiDefinitionId,
        [FromQuery] Guid companyId,
        [FromQuery] int lookbackDays = 90,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GenerateKpiNarrativeReportQuery(companyId, kpiDefinitionId, lookbackDays), cancellationToken);
        return Ok(result);
    }
}
