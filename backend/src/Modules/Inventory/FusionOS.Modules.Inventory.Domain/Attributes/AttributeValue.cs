using FusionOS.SharedKernel;
using FusionOS.Modules.Inventory.Domain.Attributes.Events;

namespace FusionOS.Modules.Inventory.Domain.Attributes;

/// <summary>
/// One controlled value (e.g. "Red") belonging to an AttributeDefinition
/// (e.g. "Color") — the two together give the reusable, cross-product
/// attribute schema this slice adds alongside ProductVariant's existing
/// free-text Attributes string. Value is unique per AttributeDefinitionId
/// (enforced by IAttributeValueRepository.ValueExistsAsync + a DB unique
/// index on (AttributeDefinitionId, Value)) — the same value string may
/// legitimately repeat under a *different* definition (e.g. "Small" the
/// t-shirt size vs. a hypothetical "Small" package-size definition), so
/// uniqueness is not company-wide.
///
/// AttributeDefinitionId is a same-module FK, but deliberately not a
/// navigation property — this codebase has no cross-*entity* object-graph
/// precedent even within a module once two entities are each their own
/// aggregate root with their own repository (see ProductVariantAttributeValue's
/// doc comment for the fuller reasoning); the application-layer handler
/// validates the definition exists (and belongs to the same company) before
/// calling Create, the same "caller validates the opaque id" shape as
/// AddProductVariantCommandHandler validating ProductId.
///
/// Is its own TenantAggregateRoot (not an owned child collection under
/// AttributeDefinition) because the task's shape wants full CQRS + its own
/// repository — a real, independently queryable/paginated master-data list
/// (List-by-definition), not a handful of items always loaded with their
/// parent the way ProductUnitOfMeasureConversion is with Product.
/// </summary>
public sealed class AttributeValue : TenantAggregateRoot
{
    public Guid AttributeDefinitionId { get; private set; }
    public string Value { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private AttributeValue() { }

    public static AttributeValue Create(Guid companyId, Guid attributeDefinitionId, string value)
    {
        if (attributeDefinitionId == Guid.Empty)
            throw new ArgumentException("Attribute definition id is required.", nameof(attributeDefinitionId));
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Attribute value is required.", nameof(value));

        var attributeValue = new AttributeValue
        {
            CompanyId = companyId,
            AttributeDefinitionId = attributeDefinitionId,
            Value = value.Trim(),
        };

        attributeValue.Raise(new AttributeValueCreated(attributeValue.Id, companyId, attributeDefinitionId, attributeValue.Value));
        return attributeValue;
    }

    public void Deactivate() => IsActive = false;
}
