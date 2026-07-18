using FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CancelVendorReturn;
using FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CompleteVendorReturn;
using FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CreateVendorReturn;
using FusionOS.Modules.Procurement.Application.VendorReturns.Queries.ListVendorReturns;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Procurement.Api.Controllers;

/// <summary>
/// Phase 1 closeout: sends a Product back to the supplier against a
/// PurchaseOrder, Pending → Completed (Inventory debits stock asynchronously
/// via VendorReturnCompleted) or Cancelled. No GetById — only list, same
/// simplification as Inventory's ReservationsController/TransfersController.
/// </summary>
[ApiController]
[Route("api/v1/procurement/vendor-returns")]
public sealed class VendorReturnsController : ControllerBase
{
    private readonly ISender _sender;

    public VendorReturnsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateVendorReturnRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateVendorReturnCommand(request.CompanyId, request.PurchaseOrderId, request.ProductId, request.WarehouseId, request.Quantity, request.Reason);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? purchaseOrderId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListVendorReturnsQuery(companyId, purchaseOrderId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] VendorReturnActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompleteVendorReturnCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] VendorReturnActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelVendorReturnCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateVendorReturnRequest(Guid CompanyId, Guid PurchaseOrderId, Guid ProductId, Guid WarehouseId, decimal Quantity, string Reason);

public sealed record VendorReturnActionRequest(Guid CompanyId);
