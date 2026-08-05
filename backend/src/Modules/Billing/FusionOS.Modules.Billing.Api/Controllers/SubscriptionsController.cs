using FusionOS.Modules.Billing.Application.Subscriptions.Commands.ActivateSubscription;
using FusionOS.Modules.Billing.Application.Subscriptions.Commands.CancelSubscription;
using FusionOS.Modules.Billing.Application.Subscriptions.Commands.CreateSubscription;
using FusionOS.Modules.Billing.Application.Subscriptions.Queries.GetSubscriptionByCompany;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Billing.Api.Controllers;

[ApiController]
[Route("api/v1/billing/subscriptions")]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly ISender _sender;

    public SubscriptionsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSubscriptionCommand(request.CompanyId, request.PlanId, request.TrialDays);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByCompany), new { companyId = request.CompanyId }, result);
    }

    [HttpGet("by-company/{companyId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCompany(Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSubscriptionByCompanyQuery(companyId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, [FromBody] SubscriptionActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActivateSubscriptionCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] SubscriptionActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelSubscriptionCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateSubscriptionRequest(Guid CompanyId, Guid PlanId, int? TrialDays);

public sealed record SubscriptionActionRequest(Guid CompanyId);
