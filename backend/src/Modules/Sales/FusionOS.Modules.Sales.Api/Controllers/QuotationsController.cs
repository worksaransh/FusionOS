using System.Text;
using FusionOS.BuildingBlocks.Application.Csv;
using FusionOS.Modules.Sales.Application.Quotations.Commands.AcceptQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Commands.ConvertQuotationToSalesOrder;
using FusionOS.Modules.Sales.Application.Quotations.Commands.CreateQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Commands.RejectQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Queries.ListQuotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>Quotations — a pre-Sales-Order stage, convertible into a real Sales Order once accepted (docs/IMPLEMENTATION_PLAN.md Phase 10 item 8).</summary>
[ApiController]
[Route("api/v1/sales/quotations")]
public sealed class QuotationsController : ControllerBase
{
    private readonly ISender _sender;

    public QuotationsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateQuotationCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = command.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? format = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListQuotationsQuery(companyId, page, pageSize), cancellationToken);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = CsvWriter.Write(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "quotations.csv");
        }
        return Ok(result);
    }

    [HttpPost("{id:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Accept(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AcceptQuotationCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RejectQuotationCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/convert")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Convert(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ConvertQuotationToSalesOrderCommand(companyId, id), cancellationToken);
        return Ok(result);
    }
}
