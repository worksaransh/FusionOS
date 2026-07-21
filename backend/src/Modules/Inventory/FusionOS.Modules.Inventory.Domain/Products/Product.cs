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
    private readonly List<ProductUnitOfMeasureConversion> _unitOfMeasureConversions = new();
    private readonly List<ProductVariant> _variants = new();

    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string UnitOfMeasure { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public string? Barcode { get; private set; }
    public IReadOnlyList<ProductUnitOfMeasureConversion> UnitOfMeasureConversions => _unitOfMeasureConversions.AsReadOnly();
    public IReadOnlyList<ProductVariant> Variants => _variants.AsReadOnly();

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

    /// <summary>
    /// Covers the same descriptive fields Create accepts, excluding Sku — the
    /// SKU is the product's business key (uniqueness-checked at creation) and
    /// stays immutable after creation, matching Warehouse.Code/Zone.Code.
    /// </summary>
    public void UpdateDetails(string name, string unitOfMeasure, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new ArgumentException("Unit of measure is required.", nameof(unitOfMeasure));

        Name = name.Trim();
        UnitOfMeasure = unitOfMeasure.Trim().ToUpperInvariant();
        Description = description;
    }

    /// <summary>
    /// Adds (or, if the same alternate unit already exists, replaces — an upsert,
    /// same "record what's true now" semantics used elsewhere in this codebase
    /// for corrections) a Multi-UOM conversion (M9 remaining, 2026-07-16).
    /// </summary>
    public void AddUnitOfMeasureConversion(string alternateUnitOfMeasure, decimal conversionFactor)
    {
        var conversion = ProductUnitOfMeasureConversion.Create(alternateUnitOfMeasure, conversionFactor);
        if (conversion.AlternateUnitOfMeasure == UnitOfMeasure)
            throw new ArgumentException("Alternate unit of measure cannot be the same as the product's own base unit of measure.", nameof(alternateUnitOfMeasure));

        var existing = _unitOfMeasureConversions.FirstOrDefault(c => c.AlternateUnitOfMeasure == conversion.AlternateUnitOfMeasure);
        if (existing is not null)
            _unitOfMeasureConversions.Remove(existing);

        _unitOfMeasureConversions.Add(conversion);
    }

    public void RemoveUnitOfMeasureConversion(string alternateUnitOfMeasure)
    {
        var normalized = alternateUnitOfMeasure?.Trim().ToUpperInvariant();
        var existing = _unitOfMeasureConversions.FirstOrDefault(c => c.AlternateUnitOfMeasure == normalized)
            ?? throw new ArgumentException($"No unit-of-measure conversion found for '{alternateUnitOfMeasure}'.", nameof(alternateUnitOfMeasure));

        _unitOfMeasureConversions.Remove(existing);
    }

    /// <summary>Adds a new variant SKU (e.g. a specific color/size combination) — see ProductVariant's own doc comment for why Attributes is free-form.</summary>
    public void AddVariant(string variantSku, string attributes)
    {
        var variant = ProductVariant.Create(variantSku, attributes);
        if (variant.VariantSku == Sku)
            throw new ArgumentException("Variant SKU cannot be the same as the product's own base SKU.", nameof(variantSku));
        if (_variants.Any(v => v.VariantSku == variant.VariantSku))
            throw new ArgumentException($"Variant SKU '{variant.VariantSku}' already exists on this product.", nameof(variantSku));

        _variants.Add(variant);
    }

    public void DeactivateVariant(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new ArgumentException($"No variant found with id '{variantId}'.", nameof(variantId));

        variant.Deactivate();
    }

    /// <summary>
    /// Assigns (or, passed null/blank, clears) the single canonical barcode/QR-payload VALUE
    /// for this product (2026-07-21 barcode/QR support — FusionOS previously had none at all).
    ///
    /// Deliberate scope decision, spelled out here rather than left implicit: this field and its
    /// lookup (GetProductByBarcodeQuery) are the whole of what FusionOS owns for barcode/QR — a
    /// canonical string value per product, and a fast reverse lookup from that value back to the
    /// product (the real "USB scanner gun types the code + Enter into a search box" warehouse
    /// workflow). It deliberately does NOT render every possible symbology:
    ///   - A linear barcode (Code 39) IS rendered client-side from this same value
    ///     (frontend/src/shared/barcode/code39.ts + BarcodeLabel.tsx), because Code 39's
    ///     character-to-bar-pattern mapping is a small, fully-specified, tractable-to-implement-
    ///     correctly table.
    ///   - A real QR code is a structured 2D matrix with Reed-Solomon error correction. Writing a
    ///     correct encoder from scratch with no library is a substantial, error-prone undertaking
    ///     — get it subtly wrong and you get an image that LOOKS like a QR code but silently
    ///     fails to scan, which is worse than not having one at all. So FusionOS does not attempt
    ///     one. The same Barcode string this method stores IS the QR payload — rendering it as an
    ///     actual QR symbol is deferred to either (a) a future dependency-backed implementation
    ///     (a maintained QR-encoding library), or (b) the label printer/software the warehouse
    ///     already owns, which can encode this exact value into whatever symbology (QR, Code 128,
    ///     etc.) it needs. This method's job is only to own the value.
    /// </summary>
    public void AssignBarcode(string? barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            Barcode = null;
            return;
        }

        var trimmed = barcode.Trim();
        if (trimmed.Length > 64)
            throw new ArgumentException("Barcode must be 64 characters or fewer.", nameof(barcode));

        Barcode = trimmed;
    }
}
