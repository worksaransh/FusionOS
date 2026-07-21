using FusionOS.Modules.Inventory.Application.Batches.Commands.ConsumeBatch;
using FusionOS.Modules.Inventory.Application.Batches.Commands.CreateBatch;
using FusionOS.Modules.Inventory.Application.Batches.Commands.UpdateBatchExpiry;
using FusionOS.Modules.Inventory.Application.Batches.Queries.GetBatchById;
using FusionOS.Modules.Inventory.Application.Batches.Queries.ListBatchesByProduct;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Inventory.Api.Controllers;

/// <summary>
/// Structured batch/lot tracking, alongside (not replacing) the existing
/// opaque InventoryLedgerEntry.BatchNumber/GoodsReceiptLine.BatchNumber
/// string fields — see Batch.cs's doc comment.
/// </summary>
[ApiController]
[Route("api/v1/inventory/batches")]
public sealed class BatchesController : ControllerBase
{
    private readonly ISender _sender;

    public BatchesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateBatchRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateBatchCommand(request.CompanyId, request.ProductId, request.BatchNumber, request.QuantityReceived, request.ExpiryDate);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBatchByIdQuery(companyId, id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid companyId,
        [FromQuery] Guid productId,
        [FromQuery] DateTimeOffset? expiringBefore = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListBatchesByProductQuery(companyId, productId, expiringBefore, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/consume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Consume(Guid id, [FromBody] ConsumeBatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ConsumeBatchCommand(request.CompanyId, id, request.Quantity), cancellationToken);
        return Ok(result);
    }

    // Modeled as a POST action, not a PUT/DELETE — this codebase's apiClient has no delete method, and this
    // isn't a full-resource replace, same "action, not CRUD update" convention as Reservation's release/fulfill.
    [HttpPost("{id:guid}/expiry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustExpiry(Guid id, [FromBody] AdjustBatchExpiryRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateBatchExpiryCommand(request.CompanyId, id, request.NewExpiry), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateBatchRequest(Guid CompanyId, Guid ProductId, string BatchNumber, decimal QuantityReceived, DateTimeOffset? ExpiryDate);

public sealed record ConsumeBatchRequest(Guid CompanyId, decimal Quantity);

public sealed record AdjustBatchExpiryRequest(Guid CompanyId, DateTimeOffset? NewExpiry);
