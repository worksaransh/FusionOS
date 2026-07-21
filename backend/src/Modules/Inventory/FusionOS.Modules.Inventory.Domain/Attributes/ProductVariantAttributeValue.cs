using FusionOS.SharedKernel;

namespace FusionOS.Modules.Inventory.Domain.Attributes;

/// <summary>
/// Records that one ProductVariant carries one AttributeValue for one
/// AttributeDefinition — e.g. variant "TSHIRT-RED-M" carries AttributeValue
/// "Red" (definition "Color") and AttributeValue "M" (definition "Size").
///
/// Modeling choice (the task explicitly asks this be justified): this is its
/// own small aggregate referencing ProductId/VariantId/AttributeDefinitionId/
/// AttributeValueId by opaque Guid, *not* a collection owned by ProductVariant
/// itself, for three concrete reasons found by reading Product.cs/
/// ProductVariant.cs/ProductRepository.cs before choosing:
///
/// 1. ProductVariant is deliberately a plain child Entity (no CompanyId/own
///    repository — "no audit/tenant columns of its own", per its own doc
///    comment) whose only mutators are internal, called solely by Product.
///    Reaching into it from a *different* feature slice (Attributes) would
///    mean either widening ProductVariant's internal API just for this, or
///    routing every attribute-assignment operation through Product's
///    aggregate root/repository (IProductRepository) even though the
///    operation conceptually belongs to the Attributes slice, not Product
///    master-data maintenance.
/// 2. Confirmed by reading it: ProductRepository.GetByIdAsync/Filtered() only
///    `.Include(p => p.UnitOfMeasureConversions)` — Variants is not included
///    on read paths today. Adding a `ProductVariantAttributeValue` collection
///    *onto* ProductVariant would inherit that same gap (silently-empty
///    attribute lists on every list/get) unless every one of those read paths
///    were also changed — a wider, riskier edit to shared, actively-touched
///    files than this slice should make.
/// 3. This codebase's own stated preference, confirmed by ProductVariant's
///    doc comment on why variant-level stock isn't threaded through the
///    Ledger yet ("a bigger schema change... deliberately not attempted") and
///    by how Reservation/Recommendation/Integration Hub reference other
///    aggregates by plain, unvalidated-at-the-domain-layer opaque ids rather
///    than object references: cross-aggregate links are opaque ids checked at
///    the application layer, not deep object graphs.
///
/// Consequently Product.cs and ProductVariant.cs need zero changes for this
/// feature — the assignment's own repository/handlers independently validate
/// (a) the Product+Variant pair exists (IProductRepository.VariantExistsAsync,
/// a direct query — see its own doc comment for why that, not Include, is
/// used) and (b) the AttributeValue exists, before creating this record.
///
/// One value per definition per variant: AssignAttributeValueToVariant is an
/// upsert — assigning a second value under a definition the variant already
/// has replaces the first — same "record what's true now" semantics as
/// Product.AddUnitOfMeasureConversion. Removal is a real delete (not a
/// soft-deactivate flag) because this is a join/line-item record, not
/// master data — same tier and precedent as
/// Product.RemoveUnitOfMeasureConversion's hard removal of the child row.
/// </summary>
public sealed class ProductVariantAttributeValue : TenantAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public Guid AttributeDefinitionId { get; private set; }
    public Guid AttributeValueId { get; private set; }

    private ProductVariantAttributeValue() { }

    public static ProductVariantAttributeValue Create(Guid companyId, Guid productId, Guid variantId, Guid attributeDefinitionId, Guid attributeValueId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (variantId == Guid.Empty)
            throw new ArgumentException("Variant id is required.", nameof(variantId));
        if (attributeDefinitionId == Guid.Empty)
            throw new ArgumentException("Attribute definition id is required.", nameof(attributeDefinitionId));
        if (attributeValueId == Guid.Empty)
            throw new ArgumentException("Attribute value id is required.", nameof(attributeValueId));

        return new ProductVariantAttributeValue
        {
            CompanyId = companyId,
            ProductId = productId,
            VariantId = variantId,
            AttributeDefinitionId = attributeDefinitionId,
            AttributeValueId = attributeValueId,
        };
    }
}
