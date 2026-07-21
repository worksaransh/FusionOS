using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.AssignAttributeValueToVariant;

/// <summary>
/// Upsert — assigning a value under a definition the variant already carries
/// a value for replaces it. See ProductVariantAttributeValue's doc comment
/// for the full modeling rationale (a separate small aggregate referencing
/// Product/Variant/AttributeValue by opaque id, not a collection owned by
/// ProductVariant). Reuses "inventory.attribute.update" — no separate
/// permission code was reserved for the assignment operation itself, and this
/// is squarely "changing which attribute values apply," the same tier as
/// editing the attribute universe.
/// </summary>
public sealed record AssignAttributeValueToVariantCommand(Guid CompanyId, Guid ProductId, Guid VariantId, Guid AttributeValueId)
    : ICommand<ProductVariantAttributeValueDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.attribute.update" };
    public string EntityType => nameof(Domain.Attributes.ProductVariantAttributeValue);
    public Guid EntityId { get; init; }
    public string Action => "Assigned";
}
