using System.Text;
using FusionOS.BuildingBlocks.Application.Csv;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.ClearSalesOrderLineBackorder;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.ConfirmSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.FlagSalesOrderLineBackordered;
using FusionOS.Modules.Sales.Application.SalesOrders.Queries.ListSalesOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>Sales Orders — next slice after Customer (08_API_STANDARDS.md).</summary>
[ApiController]
[Route("api/v1/sales/sales-orders")]
public sealed class SalesOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public SalesOrdersController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = command.CompanyId }, result);
    }

    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ConfirmSalesOrderCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/lines/{lineId:guid}/flag-backordered")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagLineBackordered(Guid id, Guid lineId, [FromBody] FlagBackorderedRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new FlagSalesOrderLineBackorderedCommand(request.CompanyId, id, lineId, request.BackorderedQuantity), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/lines/{lineId:guid}/clear-backorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearLineBackorder(Guid id, Guid lineId, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ClearSalesOrderLineBackorderCommand(companyId, id, lineId), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? format = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListSalesOrdersQuery(companyId, page, pageSize), cancellationToken);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = CsvWriter.Write(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "sales-orders.csv");
        }
        return Ok(result);
    }
}

public sealed record FlagBackorderedRequest(Guid CompanyId, decimal BackorderedQuantity);
