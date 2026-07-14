using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Queries.ListPurchaseOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Procurement.Api.Controllers;

/// <summary>Purchase Orders — next slice after Supplier (08_API_STANDARDS.md). Approval is modeled as a sub-resource action per 08_API_STANDARDS.md §3.</summary>
[ApiController]
[Route("api/v1/procurement/purchase-orders")]
public sealed class PurchaseOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public PurchaseOrdersController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = command.CompanyId }, result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ApprovePurchaseOrderCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListPurchaseOrdersQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
