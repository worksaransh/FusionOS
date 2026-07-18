namespace FusionOS.Modules.Inventory.Domain.Products;

/// <summary>
/// One sellable/stockable variation of a Product — 05_MODULE_ROADMAP.md's
/// Inventory "Variants" line item (Phase 1 closeout, 2026-07-18), confirmed
/// absent by a repo-wide grep before this slice. E.g. a T-shirt Product with
/// variants "TSHIRT-RED-M" ("Color: Red, Size: M"), "TSHIRT-BLUE-L", etc.
///
/// Attributes is a single free-form descriptive string, not a structured
/// key/value attribute set — a real attribute schema (Color/Size/Material as
/// their own typed dimensions, shared across products) is a larger, separately
/// -scoped piece of catalog design this narrow slice doesn't attempt to
/// half-build; same restraint as AI's Recommendation.Type and Integration
/// Hub's Provider being plain strings rather than a premature enum/schema.
///
/// Same "line item owned entirely by its parent aggregate" shape as
/// ProductUnitOfMeasureConversion — no audit/tenant columns of its own
/// (04_DATABASE_GUIDELINES.md §3 documented exception), lifecycle owned by
/// Product, mutators are internal (only Product itself calls them).
///
/// This does NOT give each variant its own row in the Inventory Ledger — stock
/// is still tracked against the parent Product's own id. Variant-level stock
/// tracking (the ledger keying on ProductId+VariantSku instead of just
/// ProductId) is a bigger schema change to the Ledger itself, deliberately not
/// attempted in this pass; this slice covers variant *identity* (SKU/
/// Attributes), not variant-level stock.
/// </summary>
public sealed class ProductVariant
{
    public Guid Id { get; private set; }
    public string VariantSku { get; private set; } = default!;
    public string Attributes { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private ProductVariant() { }

    internal static ProductVariant Create(string variantSku, string attributes)
    {
        if (string.IsNullOrWhiteSpace(variantSku))
            throw new ArgumentException("Variant SKU is required.", nameof(variantSku));
        if (string.IsNullOrWhiteSpace(attributes))
            throw new ArgumentException("Variant attributes are required.", nameof(attributes));

        return new ProductVariant
        {
            Id = Guid.NewGuid(),
            VariantSku = variantSku.Trim().ToUpperInvariant(),
            Attributes = attributes.Trim(),
        };
    }

    internal void Deactivate() => IsActive = false;
}
