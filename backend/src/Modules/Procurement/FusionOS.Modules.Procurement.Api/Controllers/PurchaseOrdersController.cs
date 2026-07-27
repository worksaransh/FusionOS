using System.Text;
using FusionOS.BuildingBlocks.Application.Csv;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CancelPurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.RejectPurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.SubmitPurchaseOrderForApproval;
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

    /// <summary>
    /// Submits a Draft purchase order into Core's generic Approval workflow
    /// engine (2026-07-22) — an additive alternative to POST {id}/approve
    /// above, which still works exactly as before for whatever calls it
    /// directly. Approvers are resolved on Core's side once
    /// PurchaseOrderSubmittedForApprovalConsumer reacts to the resulting
    /// integration event; there's nothing further to pass here.
    /// </summary>
    [HttpPost("{id:guid}/submit-for-approval")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitForApproval(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SubmitPurchaseOrderForApprovalCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancels a purchase order directly (Phase 10 remediation, 2026-07-27) —
    /// the long-known gap where Cancel had no route/command/domain method at
    /// all. Deliberately separate from Reject below: this models the business
    /// voiding the order before it's complete, not an approver turning it down.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelPurchaseOrderCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Rejects a purchase order directly (Phase 10 remediation, 2026-07-27) —
    /// an additive manual counterpart to the existing internal
    /// ApprovalRequestRejectedConsumer path, exposing Reject() outside of
    /// Core's Approval workflow engine too.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RejectPurchaseOrderCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? format = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListPurchaseOrdersQuery(companyId, page, pageSize), cancellationToken);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = CsvWriter.Write(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "purchase-orders.csv");
        }
        return Ok(result);
    }
}
