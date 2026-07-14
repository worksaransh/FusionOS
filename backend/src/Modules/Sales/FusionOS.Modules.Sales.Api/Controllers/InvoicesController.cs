using FusionOS.Modules.Sales.Application.Invoices.Commands.CreateInvoice;
using FusionOS.Modules.Sales.Application.Invoices.Commands.IssueInvoice;
using FusionOS.Modules.Sales.Application.Invoices.Queries.ListInvoices;
using FusionOS.Modules.Sales.Domain.Invoices;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>Invoices — next slice after Sales Orders (05_MODULE_ROADMAP.md Phase 1: Sales capability list — "Invoice").</summary>
[ApiController]
[Route("api/v1/sales/invoices")]
public sealed class InvoicesController : ControllerBase
{
    private readonly ISender _sender;

    public InvoicesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new InvoiceLineInput(l.ProductId, l.Quantity, l.UnitPrice)).ToList();
        var command = new CreateInvoiceCommand(request.CompanyId, request.SalesOrderId, request.CustomerId, lines);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListInvoicesQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/issue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Issue(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new IssueInvoiceCommand(companyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateInvoiceLineRequest(Guid ProductId, decimal Quantity, decimal UnitPrice);

public sealed record CreateInvoiceRequest(Guid CompanyId, Guid SalesOrderId, Guid CustomerId, IReadOnlyList<CreateInvoiceLineRequest> Lines);
