using FusionOS.Modules.Sales.Application.SalesOrders.Commands.ConfirmSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;
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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListSalesOrdersQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
