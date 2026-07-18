using FusionOS.Modules.Inventory.Application.Transfers.Commands.CancelTransfer;
using FusionOS.Modules.Inventory.Application.Transfers.Commands.CompleteTransfer;
using FusionOS.Modules.Inventory.Application.Transfers.Commands.CreateTransfer;
using FusionOS.Modules.Inventory.Application.Transfers.Queries.ListTransfers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Inventory.Api.Controllers;

/// <summary>
/// Phase 1 closeout: moves a Product's stock from one Warehouse to another,
/// Pending → Completed (posts the two ledger entries) or Cancelled. No
/// GetById — only list, same simplification as ReservationsController.
/// </summary>
[ApiController]
[Route("api/v1/inventory/transfers")]
public sealed class TransfersController : ControllerBase
{
    private readonly ISender _sender;

    public TransfersController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateTransferRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTransferCommand(request.CompanyId, request.ProductId, request.SourceWarehouseId, request.DestinationWarehouseId, request.Quantity);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? productId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListTransfersQuery(companyId, productId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] TransferActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompleteTransferCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] TransferActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelTransferCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateTransferRequest(Guid CompanyId, Guid ProductId, Guid SourceWarehouseId, Guid DestinationWarehouseId, decimal Quantity);

public sealed record TransferActionRequest(Guid CompanyId);
