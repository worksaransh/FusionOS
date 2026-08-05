using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.CreateKpiDefinition;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.DeactivateKpiDefinition;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.UpdateKpiDefinition;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Queries.GetKpiDefinitionById;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Queries.ListKpiDefinitions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Queries.GetLiveKpiValue;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.BusinessIntelligence.Api.Controllers;

/// <summary>
/// Phase 6 — Business Intelligence: the KPI catalog. CRUD-ish shape (create/read/list/
/// soft-deactivate) mirroring CostCentersController/AssetsController/EmployeesController.
/// </summary>
[ApiController]
[Route("api/v1/bi/kpi-definitions")]
public sealed class KpiDefinitionsController : ControllerBase
{
    private readonly ISender _sender;

    public KpiDefinitionsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateKpiDefinitionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateKpiDefinitionCommand(request.CompanyId, request.Code, request.Name, request.Unit);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetKpiDefinitionByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListKpiDefinitionsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateKpiDefinitionRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateKpiDefinitionCommand(request.CompanyId, id, request.Name, request.Unit);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateKpiDefinitionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateKpiDefinitionCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Phase 2.3 — the KPI's current value computed live from operational data
    /// (via IKpiDataSource) instead of the most recent manually-recorded
    /// bi.kpi-snapshot.record value. Response includes ComputedAt as the "as of"
    /// freshness timestamp.
    /// </summary>
    [HttpGet("{id:guid}/live-value")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLiveValue(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetLiveKpiValueQuery(companyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateKpiDefinitionRequest(Guid CompanyId, string Code, string Name, string? Unit);

public sealed record UpdateKpiDefinitionRequest(Guid CompanyId, string Name, string? Unit);

public sealed record DeactivateKpiDefinitionRequest(Guid CompanyId);
