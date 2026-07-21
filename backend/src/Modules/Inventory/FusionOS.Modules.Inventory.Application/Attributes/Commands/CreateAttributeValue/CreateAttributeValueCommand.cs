using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeValue;

public sealed record CreateAttributeValueCommand(Guid CompanyId, Guid AttributeDefinitionId, string Value)
    : ICommand<AttributeValueDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.attribute.create" };
    public string EntityType => nameof(Domain.Attributes.AttributeValue);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
