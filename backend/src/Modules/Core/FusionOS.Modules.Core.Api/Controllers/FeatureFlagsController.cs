using FusionOS.Modules.Core.Application.FeatureFlags.Commands.CreateFeatureFlag;
using FusionOS.Modules.Core.Application.FeatureFlags.Commands.ToggleFeatureFlag;
using FusionOS.Modules.Core.Application.FeatureFlags.Commands.UpdateFeatureFlag;
using FusionOS.Modules.Core.Application.FeatureFlags.Queries.GetFeatureFlagById;
using FusionOS.Modules.Core.Application.FeatureFlags.Queries.IsFeatureEnabled;
using FusionOS.Modules.Core.Application.FeatureFlags.Queries.ListFeatureFlags;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// Net-new per-company feature flags (no prior FeatureManagement library or flag table
/// anywhere in this codebase). Create/Update/Toggle require "core.feature-flag.manage";
/// List/GetById/Evaluate require "core.feature-flag.read" (both enforced in the
/// command/query itself via IRequirePermission, not here). Evaluate is the actual runtime
/// entry point other code (and other modules, via IFeatureFlagService) would use to ask
/// "is this on right now" — GET rather than POST since it has no side effects.
/// </summary>
[ApiController]
[Route("api/v1/core/feature-flags")]
public sealed class FeatureFlagsController : ControllerBase
{
    private readonly ISender _sender;

    public FeatureFlagsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateFeatureFlagCommand(request.CompanyId, request.Key, request.Name, request.Description, request.RolloutPercentage);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetFeatureFlagByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListFeatureFlagsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateFeatureFlagCommand(request.CompanyId, id, request.Name, request.Description, request.RolloutPercentage);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/toggle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Toggle(Guid id, [FromBody] ToggleFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ToggleFeatureFlagCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    // GET, not POST — evaluation has no side effects. evaluationId is optional: supplied,
    // it hashes that caller into/out of the rollout percentage; omitted, it's a plain
    // on/off read of the flag as a whole (see FeatureFlag.Evaluate).
    [HttpGet("evaluate/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Evaluate(string key, [FromQuery] Guid companyId, [FromQuery] string? evaluationId = null, CancellationToken cancellationToken = default)
    {
        var isEnabled = await _sender.Send(new IsFeatureEnabledQuery(companyId, key, evaluationId), cancellationToken);
        return Ok(new FeatureFlagEvaluationResult(key, isEnabled));
    }
}

public sealed record CreateFeatureFlagRequest(Guid CompanyId, string Key, string Name, string? Description, int RolloutPercentage = 100);

public sealed record UpdateFeatureFlagRequest(Guid CompanyId, string Name, string? Description, int RolloutPercentage);

public sealed record ToggleFeatureFlagRequest(Guid CompanyId);

public sealed record FeatureFlagEvaluationResult(string Key, bool IsEnabled);
