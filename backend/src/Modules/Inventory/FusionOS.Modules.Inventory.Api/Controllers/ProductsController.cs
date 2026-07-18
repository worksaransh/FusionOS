using System.Text;
using FusionOS.BuildingBlocks.Application.Csv;
using FusionOS.Modules.Inventory.Application.Products.Commands.AddProductVariant;
using FusionOS.Modules.Inventory.Application.Products.Commands.AddUnitOfMeasureConversion;
using FusionOS.Modules.Inventory.Application.Products.Commands.CreateProduct;
using FusionOS.Modules.Inventory.Application.Products.Commands.DeactivateProduct;
using FusionOS.Modules.Inventory.Application.Products.Commands.DeactivateProductVariant;
using FusionOS.Modules.Inventory.Application.Products.Commands.RemoveUnitOfMeasureConversion;
using FusionOS.Modules.Inventory.Application.Products.Commands.UpdateProduct;
using FusionOS.Modules.Inventory.Application.Products.Queries.GetProductById;
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

    // Wired up (2026-07-14) — was a dead stub that always returned NotFound;
    // see CompaniesController for the same documented gap this fixes.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProductByIdQuery(companyId, id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] string? format = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListProductsQuery(companyId, search, page, pageSize), cancellationToken);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = CsvWriter.Write(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "products.csv");
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(request.CompanyId, id, request.Name, request.UnitOfMeasure, request.Description);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a DELETE, this never removes the row
    // (08_API_STANDARDS.md / 04_DATABASE_GUIDELINES.md).
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateProductCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    // M9-remaining e: Multi-UOM (2026-07-16). Upsert semantics — adding a conversion for an
    // alternate unit that already exists replaces it, per PickListLine/CycleCount precedent.
    [HttpPost("{id:guid}/unit-of-measure-conversions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddUnitOfMeasureConversion(Guid id, [FromBody] AddUnitOfMeasureConversionRequest request, CancellationToken cancellationToken)
    {
        var command = new AddUnitOfMeasureConversionCommand(request.CompanyId, id, request.AlternateUnitOfMeasure, request.ConversionFactor);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Modeled as a POST action, not a DELETE — this codebase's apiClient has no delete method by design.
    [HttpPost("{id:guid}/unit-of-measure-conversions/remove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveUnitOfMeasureConversion(Guid id, [FromBody] RemoveUnitOfMeasureConversionRequest request, CancellationToken cancellationToken)
    {
        var command = new RemoveUnitOfMeasureConversionCommand(request.CompanyId, id, request.AlternateUnitOfMeasure);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Phase 1 closeout (2026-07-18): Variants.
    [HttpPost("{id:guid}/variants")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddVariant(Guid id, [FromBody] AddProductVariantRequest request, CancellationToken cancellationToken)
    {
        var command = new AddProductVariantCommand(request.CompanyId, id, request.VariantSku, request.Attributes);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — same convention as Product.Deactivate itself.
    [HttpPost("{id:guid}/variants/{variantId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateVariant(Guid id, Guid variantId, [FromBody] DeactivateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new DeactivateProductVariantCommand(request.CompanyId, id, variantId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

/// <summary>Request body for product update — Id comes from the route, not the body.</summary>
public sealed record UpdateProductRequest(Guid CompanyId, string Name, string UnitOfMeasure, string? Description);

/// <summary>Request body for product deactivation — just carries CompanyId for tenant scoping.</summary>
public sealed record DeactivateProductRequest(Guid CompanyId);

/// <summary>Request body for adding/upserting a unit-of-measure conversion.</summary>
public sealed record AddUnitOfMeasureConversionRequest(Guid CompanyId, string AlternateUnitOfMeasure, decimal ConversionFactor);

/// <summary>Request body for removing a unit-of-measure conversion.</summary>
public sealed record RemoveUnitOfMeasureConversionRequest(Guid CompanyId, string AlternateUnitOfMeasure);

/// <summary>Request body for adding a variant SKU.</summary>
public sealed record AddProductVariantRequest(Guid CompanyId, string VariantSku, string Attributes);
