namespace FusionOS.Modules.Inventory.Application.Products.Contracts;

/// <summary>One Multi-UOM conversion (M9 remaining, 2026-07-16) — see ProductUnitOfMeasureConversion's doc comment for the "caller converts, ledger stays in base UOM" restraint.</summary>
public sealed record UnitOfMeasureConversionDto(string AlternateUnitOfMeasure, decimal ConversionFactor);

/// <summary>One variant SKU (Phase 1 closeout, 2026-07-18) — see ProductVariant's doc comment for why Attributes is free-form and why this doesn't give the variant its own ledger presence.</summary>
public sealed record ProductVariantDto(Guid Id, string VariantSku, string Attributes, bool IsActive);

/// <summary>Published DTO — the only shape other modules (Warehouse, Procurement, Sales) will depend on once they reference products (03_SYSTEM_ARCHITECTURE.md §2).</summary>
public sealed record ProductDto(Guid Id, string Sku, string Name, string? Description, string UnitOfMeasure, bool IsActive, DateTimeOffset CreatedAt, IReadOnlyList<UnitOfMeasureConversionDto> UnitOfMeasureConversions, IReadOnlyList<ProductVariantDto> Variants);
