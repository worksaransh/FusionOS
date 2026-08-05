using FusionOS.Modules.Billing.Application.Plans.Commands.CreatePlan;
using FusionOS.Modules.Billing.Application.Plans.Queries.ListPlans;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Billing.Api.Controllers;

/// <summary>Global pricing catalog (Starter/Growth/Enterprise) — see Plan's class doc comment for why this isn't tenant-scoped.</summary>
[ApiController]
[Route("api/v1/billing/plans")]
public sealed class PlansController : ControllerBase
{
    private readonly ISender _sender;

    public PlansController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreatePlanRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePlanCommand(request.Code, request.Name, request.Tier, request.MonthlyPricePerUser, request.IncludedModules);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), null, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListPlansQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreatePlanRequest(string Code, string Name, string Tier, decimal MonthlyPricePerUser, string IncludedModules);
