using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;

namespace FusionOS.Modules.Inventory.Application.Attributes.Queries.ListVariantAttributeValues;

public sealed record ListVariantAttributeValuesQuery(Guid CompanyId, Guid ProductId, Guid VariantId)
    : IQuery<IReadOnlyList<ProductVariantAttributeValueDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.attribute.read" };
}
