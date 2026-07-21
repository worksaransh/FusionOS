using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.DeactivateAttributeValue;

/// <summary>Soft-deactivate only — never a real delete, same convention as DeactivateAttributeDefinitionCommand.</summary>
public sealed record DeactivateAttributeValueCommand(Guid CompanyId, Guid AttributeValueId)
    : ICommand<AttributeValueDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.attribute.deactivate" };
    public string EntityType => nameof(Domain.Attributes.AttributeValue);
    public Guid EntityId => AttributeValueId;
    public string Action => "Deactivated";
}
