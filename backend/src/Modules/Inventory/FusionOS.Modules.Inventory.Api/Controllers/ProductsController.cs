using System.Text;
using FusionOS.BuildingBlocks.Application.Csv;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.AssignAttributeValueToVariant;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.RemoveAttributeValueFromVariant;
using FusionOS.Modules.Inventory.Application.Attributes.Queries.ListVariantAttributeValues;
using FusionOS.Modules.Inventory.Application.Products.Commands.AddProductVariant;
using FusionOS.Modules.Inventory.Application.Products.Commands.AddUnitOfMeasureConversion;
using FusionOS.Modules.Inventory.Application.Products.Commands.AssignProductBarcode;
using FusionOS.Modules.Inventory.Application.Products.Commands.CreateProduct;
using FusionOS.Modules.Inventory.Application.Products.Commands.DeactivateProduct;
using FusionOS.Modules.Inventory.Application.Products.Commands.DeactivateProductVariant;
using FusionOS.Modules.Inventory.Application.Products.Commands.RemoveUnitOfMeasureConversion;
using FusionOS.Modules.Inventory.Application.Products.Commands.UpdateProduct;
using FusionOS.Modules.Inventory.Application.Products.Queries.GetProductByBarcode;
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

    /// <summary>Creates a new product (master reference data — no stock is created here, see StockController).</summary>
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
    /// <summary>Gets a single product by id, scoped to the given company.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProductByIdQuery(companyId, id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // Barcode/QR support (2026-07-21) — the "scanner gun types code + Enter" lookup. Placed
    // ahead of ListProducts in this file (route ordering doesn't matter here since "by-barcode"
    // can never match the {id:guid} constraint on GetById, but keeping the two GETs adjacent
    // reads naturally). Mirrors GetById's null -> 404 shape exactly.
    /// <summary>Looks up a product by its assigned barcode — the "scanner gun types code + Enter" lookup path.</summary>
    [HttpGet("by-barcode/{barcode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByBarcode(string barcode, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProductByBarcodeQuery(companyId, barcode), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Lists products for a company with optional free-text search, paginated (default page size 25). Pass format=csv to download the page as a CSV file instead of JSON.</summary>
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

    /// <summary>Updates a product's name, unit of measure and description.</summary>
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
    /// <summary>Deactivates the product. Soft-delete only — the row is never removed.</summary>
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
    /// <summary>Adds (or upserts, if the alternate unit already has one) a unit-of-measure conversion for this product.</summary>
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
    /// <summary>Removes a unit-of-measure conversion from this product.</summary>
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

    // Barcode/QR support (2026-07-21). Assigning (or, with barcode null/blank, clearing) a
    // barcode is product master-data maintenance — modeled as a POST action like the other
    // sub-resource writes on this controller, not a PATCH/DELETE (apiClient has no
    // patch/delete-with-body method by design; see AddUnitOfMeasureConversion's own comment).
    /// <summary>Assigns (or, with Barcode null/blank, clears) this product's barcode/QR-payload value.</summary>
    [HttpPost("{id:guid}/barcode")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignBarcode(Guid id, [FromBody] AssignProductBarcodeRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignProductBarcodeCommand(request.CompanyId, id, request.Barcode);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Phase 1 closeout (2026-07-18): Variants.
    /// <summary>Adds a new variant SKU to this product.</summary>
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
    /// <summary>Deactivates a variant. Soft-delete only, same convention as Product.Deactivate.</summary>
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

    // Attributes slice — assigns a structured AttributeValue (e.g. "Red" under
    // "Color") to this variant. Upsert: replaces whatever value the variant
    // already carries for that same attribute definition (see
    // ProductVariantAttributeValue.cs / AssignAttributeValueToVariantCommand.cs
    // doc comments for why this is a standalone aggregate referencing
    // Product/Variant/AttributeValue by opaque id rather than a collection
    // owned by ProductVariant itself).
    /// <summary>Assigns (upsert — replaces any existing value for the same attribute definition) a structured AttributeValue to this variant.</summary>
    [HttpPost("{id:guid}/variants/{variantId:guid}/attribute-values")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignAttributeValue(Guid id, Guid variantId, [FromBody] AssignAttributeValueRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignAttributeValueToVariantCommand(request.CompanyId, id, variantId, request.AttributeValueId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists all structured AttributeValues assigned to this variant.</summary>
    [HttpGet("{id:guid}/variants/{variantId:guid}/attribute-values")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAttributeValues(Guid id, Guid variantId, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListVariantAttributeValuesQuery(companyId, id, variantId), cancellationToken);
        return Ok(result);
    }

    // Modeled as a POST action, not a DELETE — same convention as RemoveUnitOfMeasureConversion.
    // Real delete of the join row (not a soft-deactivate) — see ProductVariantAttributeValue.cs.
    /// <summary>Removes an attribute-value assignment from a variant. Unlike Deactivate, this is a real delete of the join row.</summary>
    [HttpPost("{id:guid}/variants/{variantId:guid}/attribute-values/{assignmentId:guid}/remove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveAttributeValue(Guid id, Guid variantId, Guid assignmentId, [FromBody] DeactivateProductRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new RemoveAttributeValueFromVariantCommand(request.CompanyId, id, variantId, assignmentId), cancellationToken);
        return Ok();
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

/// <summary>Request body for assigning (or, with Barcode null/blank, clearing) a product's barcode/QR-payload value.</summary>
public sealed record AssignProductBarcodeRequest(Guid CompanyId, string? Barcode);

/// <summary>Request body for assigning a structured AttributeValue to a variant.</summary>
public sealed record AssignAttributeValueRequest(Guid CompanyId, Guid AttributeValueId);
