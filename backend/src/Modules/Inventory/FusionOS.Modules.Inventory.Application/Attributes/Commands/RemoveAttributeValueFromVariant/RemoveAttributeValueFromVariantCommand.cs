using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.RemoveAttributeValueFromVariant;

/// <summary>Real delete of the join row — see ProductVariantAttributeValue's doc comment for why (a line-item/join record, not master data).</summary>
public sealed record RemoveAttributeValueFromVariantCommand(Guid CompanyId, Guid ProductId, Guid VariantId, Guid AssignmentId)
    : ICommand, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.attribute.update" };
    public string EntityType => nameof(Domain.Attributes.ProductVariantAttributeValue);
    public Guid EntityId => AssignmentId;
    public string Action => "Removed";
}
