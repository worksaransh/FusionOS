using FusionOS.Modules.Inventory.Application.Products.Commands.CreateProduct;
using FusionOS.Modules.Inventory.Application.Products.Queries.ListProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Inventory.Api.Controllers;

/// <summary>Phase 1 vertical slice — Product identity/reference data only (08_API_STANDARDS.md).</summary>
[ApiController]
[Route("api/v1/inventory/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) => NotFound(); // next slice — see CompaniesController for the same documented gap.

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListProductsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
