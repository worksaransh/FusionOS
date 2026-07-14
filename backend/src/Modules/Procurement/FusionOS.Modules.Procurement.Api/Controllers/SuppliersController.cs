using FusionOS.Modules.Procurement.Application.Suppliers.Commands.CreateSupplier;
using FusionOS.Modules.Procurement.Application.Suppliers.Queries.ListSuppliers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Procurement.Api.Controllers;

[ApiController]
[Route("api/v1/procurement/suppliers")]
public sealed class SuppliersController : ControllerBase
{
    private readonly ISender _sender;

    public SuppliersController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierCommand command, CancellationToken cancellationToken)
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
        var result = await _sender.Send(new ListSuppliersQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
