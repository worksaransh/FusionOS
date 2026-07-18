using FusionOS.Modules.Sales.Application.Discounts.Commands.CreateDiscountRule;
using FusionOS.Modules.Sales.Application.Discounts.Commands.DeactivateDiscountRule;
using FusionOS.Modules.Sales.Application.Discounts.Queries.GetApplicableDiscount;
using FusionOS.Modules.Sales.Application.Discounts.Queries.ListDiscountRules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>
/// Phase 1 closeout: quantity-break tiers of a Product's discount schedule.
/// GetApplicableDiscount is a lookup the Sales Order creation flow calls
/// before committing a line's discount — it never auto-overrides
/// SalesOrderLine.DiscountPercentage itself (see DiscountRule's own doc
/// comment). No GetById on the rule itself — only list, same simplification
/// as Inventory's ReservationsController/TransfersController.
/// </summary>
[ApiController]
[Route("api/v1/sales/discount-rules")]
public sealed class DiscountRulesController : ControllerBase
{
    private readonly ISender _sender;

    public DiscountRulesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateDiscountRuleCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = command.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? productId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListDiscountRulesQuery(companyId, productId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("applicable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApplicableDiscount([FromQuery] Guid companyId, [FromQuery] Guid productId, [FromQuery] decimal quantity, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetApplicableDiscountQuery(companyId, productId, quantity), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DiscountRuleActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateDiscountRuleCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record DiscountRuleActionRequest(Guid CompanyId);
