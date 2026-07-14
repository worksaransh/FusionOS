using FusionOS.Modules.Sales.Application.Customers.Commands.CreateCustomer;
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) => NotFound();

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCustomersQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
