using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.DeactivateAttributeDefinition;

/// <summary>Soft-deactivate only — never a real delete (an AttributeDefinition may already be referenced by AttributeValues and by ProductVariantAttributeValue assignments).</summary>
public sealed record DeactivateAttributeDefinitionCommand(Guid CompanyId, Guid AttributeDefinitionId)
    : ICommand<AttributeDefinitionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.attribute.deactivate" };
    public string EntityType => nameof(Domain.Attributes.AttributeDefinition);
    public Guid EntityId => AttributeDefinitionId;
    public string Action => "Deactivated";
}
