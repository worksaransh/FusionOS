using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeDefinition;

public sealed record CreateAttributeDefinitionCommand(Guid CompanyId, string Name)
    : ICommand<AttributeDefinitionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.attribute.create" };
    public string EntityType => nameof(Domain.Attributes.AttributeDefinition);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
