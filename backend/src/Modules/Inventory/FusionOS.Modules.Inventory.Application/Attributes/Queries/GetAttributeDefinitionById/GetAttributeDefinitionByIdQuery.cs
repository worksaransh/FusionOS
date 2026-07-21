using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;

namespace FusionOS.Modules.Inventory.Application.Attributes.Queries.GetAttributeDefinitionById;

public sealed record GetAttributeDefinitionByIdQuery(Guid CompanyId, Guid AttributeDefinitionId)
    : IQuery<AttributeDefinitionDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.attribute.read" };
}
