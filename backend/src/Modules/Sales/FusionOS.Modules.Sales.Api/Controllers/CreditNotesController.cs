using System.Text;
using FusionOS.BuildingBlocks.Application.Csv;
using FusionOS.Modules.Sales.Application.CreditNotes.Commands.CreateCreditNote;
using FusionOS.Modules.Sales.Application.CreditNotes.Commands.IssueCreditNote;
using FusionOS.Modules.Sales.Application.CreditNotes.Queries.ListCreditNotes;
using FusionOS.Modules.Sales.Domain.CreditNotes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>Returns/credit notes — a return-from-customer flow that reverses an Invoice and issues a credit against the customer's AR balance (docs/IMPLEMENTATION_PLAN.md Phase 10 item 9).</summary>
[ApiController]
[Route("api/v1/sales/credit-notes")]
public sealed class CreditNotesController : ControllerBase
{
    private readonly ISender _sender;

    public CreditNotesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCreditNoteRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new CreditNoteLineInput(l.ProductId, l.Quantity, l.UnitPrice)).ToList();
        var command = new CreateCreditNoteCommand(request.CompanyId, request.InvoiceId, request.CustomerId, request.Reason, lines);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? format = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCreditNotesQuery(companyId, page, pageSize), cancellationToken);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = CsvWriter.Write(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "credit-notes.csv");
        }
        return Ok(result);
    }

    [HttpPost("{id:guid}/issue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Issue(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new IssueCreditNoteCommand(companyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateCreditNoteLineRequest(Guid ProductId, decimal Quantity, decimal UnitPrice);

public sealed record CreateCreditNoteRequest(Guid CompanyId, Guid InvoiceId, Guid CustomerId, string Reason, IReadOnlyList<CreateCreditNoteLineRequest> Lines);
