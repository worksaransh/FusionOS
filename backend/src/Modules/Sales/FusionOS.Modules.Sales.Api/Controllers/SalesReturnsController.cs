using System.Text;
using FusionOS.BuildingBlocks.Application.Csv;
using FusionOS.Modules.Sales.Application.Returns.Commands.ApproveSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Commands.CompleteSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Commands.CreateSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Commands.RejectSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Queries.GetSalesReturnById;
using FusionOS.Modules.Sales.Application.Returns.Queries.ListSalesReturns;
using FusionOS.Modules.Sales.Application.Returns.Queries.ListSalesReturnsByInvoice;
using FusionOS.Modules.Sales.Domain.Returns;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>
/// Sales returns (RMA) — the physical-return workflow that is a distinct concern from
/// CreditNotes: a customer's REQUEST to send goods back against an Invoice, its
/// Approval/Rejection, and — once physically inspected — its Completion. Optionally
/// followed, as a separate manual action, by issuing a CreditNote against the same
/// invoice (see CreditNote.SalesReturnId / POST /sales/credit-notes).
/// </summary>
[ApiController]
[Route("api/v1/sales/returns")]
public sealed class SalesReturnsController : ControllerBase
{
    private readonly ISender _sender;

    public SalesReturnsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateSalesReturnRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new SalesReturnLineInput(l.ProductId, l.Quantity)).ToList();
        var command = new CreateSalesReturnCommand(request.CompanyId, request.InvoiceId, request.CustomerId, request.Reason, lines);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? format = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? status = null, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListSalesReturnsQuery(companyId, page, pageSize, status), cancellationToken);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = CsvWriter.Write(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "sales-returns.csv");
        }
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSalesReturnByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-invoice/{invoiceId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByInvoice(Guid invoiceId, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListSalesReturnsByInvoiceQuery(companyId, invoiceId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ApproveSalesReturnCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectSalesReturnRequest request, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RejectSalesReturnCommand(companyId, id, request.RejectionReason), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteSalesReturnRequest request, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var completions = request.Lines.Select(l => new SalesReturnLineCompletion(l.LineId, l.Restocked, l.ConditionNotes)).ToList();
        var result = await _sender.Send(new CompleteSalesReturnCommand(companyId, id, completions), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateSalesReturnLineRequest(Guid ProductId, decimal Quantity);

public sealed record CreateSalesReturnRequest(Guid CompanyId, Guid InvoiceId, Guid CustomerId, string Reason, IReadOnlyList<CreateSalesReturnLineRequest> Lines);

public sealed record RejectSalesReturnRequest(string RejectionReason);

public sealed record CompleteSalesReturnLineRequest(Guid LineId, bool Restocked, string? ConditionNotes);

public sealed record CompleteSalesReturnRequest(IReadOnlyList<CompleteSalesReturnLineRequest> Lines);
