using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.UpdateAttributeDefinition;

public sealed record UpdateAttributeDefinitionCommand(Guid CompanyId, Guid AttributeDefinitionId, string Name)
    : ICommand<AttributeDefinitionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.attribute.update" };
    public string EntityType => nameof(Domain.Attributes.AttributeDefinition);
    public Guid EntityId => AttributeDefinitionId;
    public string Action => "Updated";
}
