namespace FusionOS.Modules.Inventory.Application.Products.Contracts;

/// <summary>Shared Product -> ProductDto mapping (M9 remaining, 2026-07-16 — extracted once the DTO grew a UnitOfMeasureConversions projection needed by all 5 handlers that return a ProductDto).</summary>
public static class ProductMapper
{
    public static ProductDto ToDto(Domain.Products.Product product) => new(
        product.Id,
        product.Sku,
        product.Name,
        product.Description,
        product.UnitOfMeasure,
        product.IsActive,
        product.CreatedAt,
        product.UnitOfMeasureConversions
            .Select(c => new UnitOfMeasureConversionDto(c.AlternateUnitOfMeasure, c.ConversionFactor))
            .ToList());
}
