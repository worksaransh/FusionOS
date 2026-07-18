using System.Text;
using FusionOS.BuildingBlocks.Application.Csv;
using FusionOS.Modules.Sales.Application.Customers.Commands.AssignPriceList;
using FusionOS.Modules.Sales.Application.Customers.Commands.CreateCustomer;
using FusionOS.Modules.Sales.Application.Customers.Commands.DeactivateCustomer;
using FusionOS.Modules.Sales.Application.Customers.Commands.UpdateCustomer;
using FusionOS.Modules.Sales.Application.Customers.Queries.GetCustomerById;
using FusionOS.Modules.Sales.Application.Customers.Queries.ListCustomers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

[ApiController]
[Route("api/v1/sales/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomerByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] string? format = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCustomersQuery(companyId, search, page, pageSize), cancellationToken);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = CsvWriter.Write(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "customers.csv");
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(request.CompanyId, id, request.Name, request.ContactEmail, request.CreditLimit);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateCustomerCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/assign-price-list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignPriceList(Guid id, [FromBody] AssignPriceListRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignPriceListCommand(request.CompanyId, id, request.PriceListId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record UpdateCustomerRequest(Guid CompanyId, string Name, string? ContactEmail, decimal CreditLimit);

public sealed record AssignPriceListRequest(Guid CompanyId, Guid? PriceListId);
