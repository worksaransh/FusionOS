using FusionOS.SharedKernel;
using FusionOS.Modules.Inventory.Domain.Attributes.Events;

namespace FusionOS.Modules.Inventory.Domain.Attributes;

/// <summary>
/// A reusable, controlled-vocabulary product dimension (e.g. "Color", "Size"),
/// shared across every Product in the tenant — the structured counterpart to
/// ProductVariant.Attributes' free-text string (see that entity's doc comment
/// for why the free-text field was deliberately narrow-scoped and why this is
/// the separately-scoped follow-up it called out). Pure master data, same
/// shape/conventions as Finance's CostCenter: Name is the unique business key
/// (case-insensitively, per company — enforced by
/// IAttributeDefinitionRepository.NameExistsAsync using ILike, backstopped by
/// a DB unique index on (CompanyId, Name)), IsActive is soft-deactivate only.
///
/// Deliberately no "data type" or validation-rule field (e.g. text/number/
/// color-swatch) — the PRD-equivalent ask here is a controlled set of named
/// values per dimension (AttributeValue), not a general-purpose form-builder
/// schema; adding one would be unrequested scope, same restraint CostCenter's
/// doc comment applies to skipping a cost-center hierarchy.
/// </summary>
public sealed class AttributeDefinition : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private AttributeDefinition() { }

    public static AttributeDefinition Create(Guid companyId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name is required.", nameof(name));

        var definition = new AttributeDefinition
        {
            CompanyId = companyId,
            Name = name.Trim(),
        };

        definition.Raise(new AttributeDefinitionCreated(definition.Id, companyId, definition.Name));
        return definition;
    }

    public void UpdateDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name is required.", nameof(name));

        Name = name.Trim();
    }

    public void Deactivate() => IsActive = false;
}
