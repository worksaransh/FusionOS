using FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Commands.RecordKpiSnapshot;
using FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Queries.ListKpiSnapshots;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.BusinessIntelligence.Api.Controllers;

/// <summary>
/// Phase 6 — Business Intelligence: manually-recorded, point-in-time KPI values —
/// the time series a dashboard chart renders.
/// </summary>
[ApiController]
[Route("api/v1/bi/kpi-snapshots")]
public sealed class KpiSnapshotsController : ControllerBase
{
    private readonly ISender _sender;

    public KpiSnapshotsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Record([FromBody] RecordKpiSnapshotRequest request, CancellationToken cancellationToken)
    {
        var command = new RecordKpiSnapshotCommand(request.CompanyId, request.KpiDefinitionId, request.Value, request.Notes);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId, kpiDefinitionId = request.KpiDefinitionId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? kpiDefinitionId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListKpiSnapshotsQuery(companyId, kpiDefinitionId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

public sealed record RecordKpiSnapshotRequest(Guid CompanyId, Guid KpiDefinitionId, decimal Value, string? Notes);
