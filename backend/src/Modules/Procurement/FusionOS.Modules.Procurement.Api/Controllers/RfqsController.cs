using System.Text;
using FusionOS.BuildingBlocks.Application.Csv;
using FusionOS.Modules.Procurement.Application.Rfqs.Commands.AwardRfq;
using FusionOS.Modules.Procurement.Application.Rfqs.Commands.ConvertRfqToPurchaseOrder;
using FusionOS.Modules.Procurement.Application.Rfqs.Commands.CreateRfq;
using FusionOS.Modules.Procurement.Application.Rfqs.Commands.SendRfq;
using FusionOS.Modules.Procurement.Application.Rfqs.Commands.SubmitSupplierQuote;
using FusionOS.Modules.Procurement.Application.Rfqs.Queries.ListRfqs;
using FusionOS.Modules.Procurement.Domain.Rfqs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Procurement.Api.Controllers;

/// <summary>
/// RFQ — the pre-PO stage named in PurchaseOrder's own doc comment as coming
/// "later" (08_API_STANDARDS.md). Award and Convert are modeled as sub-resource
/// actions per 08_API_STANDARDS.md §3, same as Approve on PurchaseOrdersController.
/// </summary>
[ApiController]
[Route("api/v1/procurement/rfqs")]
public sealed class RfqsController : ControllerBase
{
    private readonly ISender _sender;

    public RfqsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateRfqCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = command.CompanyId }, result);
    }

    [HttpPost("{id:guid}/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Send(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SendRfqCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/quotes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitSupplierQuote(Guid id, [FromBody] SubmitSupplierQuoteRequest request, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new SupplierQuoteLineInput(l.ProductId, l.Quantity, l.UnitPrice)).ToList();
        var command = new SubmitSupplierQuoteCommand(companyId, id, request.SupplierId, lines);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/award")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Award(Guid id, [FromBody] AwardRfqRequest request, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AwardRfqCommand(companyId, id, request.SupplierQuoteId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/convert")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Convert(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ConvertRfqToPurchaseOrderCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? format = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListRfqsQuery(companyId, page, pageSize), cancellationToken);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = CsvWriter.Write(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "rfqs.csv");
        }
        return Ok(result);
    }
}

public sealed record SubmitSupplierQuoteLineRequest(Guid ProductId, decimal Quantity, decimal UnitPrice);

public sealed record SubmitSupplierQuoteRequest(Guid SupplierId, IReadOnlyList<SubmitSupplierQuoteLineRequest> Lines);

public sealed record AwardRfqRequest(Guid SupplierQuoteId);
