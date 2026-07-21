using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;

namespace FusionOS.Modules.Inventory.Application.Attributes.Queries.ListAttributeValuesByDefinition;

public sealed record ListAttributeValuesByDefinitionQuery(Guid CompanyId, Guid AttributeDefinitionId)
    : IQuery<IReadOnlyList<AttributeValueDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.attribute.read" };
}
