using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.PriceLists.Contracts;

namespace FusionOS.Modules.Sales.Application.PriceLists.Queries.ListPriceLists;

public sealed record ListPriceListsQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<PriceListDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.price-list.read" };
}
