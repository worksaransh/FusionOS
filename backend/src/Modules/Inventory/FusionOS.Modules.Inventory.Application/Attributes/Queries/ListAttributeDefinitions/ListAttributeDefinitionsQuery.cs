using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;

namespace FusionOS.Modules.Inventory.Application.Attributes.Queries.ListAttributeDefinitions;

public sealed record ListAttributeDefinitionsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<AttributeDefinitionDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.attribute.read" };
}
