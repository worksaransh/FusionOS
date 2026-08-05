using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Commands.CreateIntercompanyEliminationRule;
using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Commands.DeactivateIntercompanyEliminationRule;
using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Queries.ListIntercompanyEliminationRules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// GL account CODE-pair rules (e.g. "Intercompany Receivable" vs "Intercompany
/// Payable") that GetConsolidatedTrialBalanceQuery nets to zero across a
/// consolidated group (Phase 2.4 — multi-entity/subsidiary consolidation). See
/// IntercompanyEliminationRule's class doc comment for why this is keyed by
/// account code rather than AccountId.
/// </summary>
[ApiController]
[Route("api/v1/finance/intercompany-elimination-rules")]
public sealed class IntercompanyEliminationRulesController : ControllerBase
{
    private readonly ISender _sender;

    public IntercompanyEliminationRulesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateIntercompanyEliminationRuleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateIntercompanyEliminationRuleCommand(
            request.CompanyId, request.ReceivableAccountCode, request.PayableAccountCode, request.Description, request.BranchId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListIntercompanyEliminationRulesQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateIntercompanyEliminationRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateIntercompanyEliminationRuleCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateIntercompanyEliminationRuleRequest(Guid CompanyId, string ReceivableAccountCode, string PayableAccountCode, string Description, Guid? BranchId);

public sealed record DeactivateIntercompanyEliminationRuleRequest(Guid CompanyId);
