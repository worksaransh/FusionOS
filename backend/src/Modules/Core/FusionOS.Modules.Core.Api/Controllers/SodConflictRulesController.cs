using FusionOS.Modules.Core.Application.SodConflictRules.Commands.CreateSodConflictRule;
using FusionOS.Modules.Core.Application.SodConflictRules.Commands.DeactivateSodConflictRule;
using FusionOS.Modules.Core.Application.SodConflictRules.Queries.ListSodConflictRules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// Lightweight Segregation-of-Duties (SoD) conflict rules (Phase 2.2 roadmap) — CRUD for the
/// catalog only. The actual enforcement (checking a rule against a same-user create+approve
/// attempt) lives at the specific approval-decision call sites elsewhere (see
/// ISegregationOfDutiesService and, for the concrete Purchase Order case,
/// ApprovePurchaseOrderCommandHandler in the Procurement module), not in this controller.
/// </summary>
[ApiController]
[Route("api/v1/core/sod-conflict-rules")]
public sealed class SodConflictRulesController : ControllerBase
{
    private readonly ISender _sender;

    public SodConflictRulesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateSodConflictRuleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSodConflictRuleCommand(request.CompanyId, request.ConflictingPermissionA, request.ConflictingPermissionB, request.Description);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListSodConflictRulesQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateSodConflictRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateSodConflictRuleCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateSodConflictRuleRequest(Guid CompanyId, string ConflictingPermissionA, string ConflictingPermissionB, string? Description);

public sealed record DeactivateSodConflictRuleRequest(Guid CompanyId);
