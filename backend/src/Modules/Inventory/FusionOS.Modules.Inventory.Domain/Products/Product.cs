using FusionOS.SharedKernel;
using FusionOS.Modules.Inventory.Domain.Products.Events;

namespace FusionOS.Modules.Inventory.Domain.Products;

/// <summary>
/// The anchor aggregate for Inventory (05_MODULE_ROADMAP.md Phase 1). Variants,
/// batch/serial tracking, the Inventory Ledger, and valuation (FIFO/Weighted
/// Average) build on top of Product in later slices — this is deliberately the
/// first, narrow cut: SKU identity and reference data, not stock movement yet.
/// </summary>
public sealed class Product : TenantAggregateRoot
{
    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string UnitOfMeasure { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Product() { }

    public static Product Create(Guid companyId, string sku, string name, string unitOfMeasure, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new ArgumentException("Unit of measure is required.", nameof(unitOfMeasure));

        var product = new Product
        {
            CompanyId = companyId,
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            UnitOfMeasure = unitOfMeasure.Trim().ToUpperInvariant(),
            Description = description,
        };

        product.Raise(new ProductCreated(product.Id, companyId, product.Sku));
        return product;
    }

    public void Deactivate() => IsActive = false;
}
