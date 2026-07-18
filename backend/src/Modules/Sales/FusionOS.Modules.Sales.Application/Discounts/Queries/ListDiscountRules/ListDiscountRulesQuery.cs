using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Discounts.Contracts;

namespace FusionOS.Modules.Sales.Application.Discounts.Queries.ListDiscountRules;

public sealed record ListDiscountRulesQuery(Guid CompanyId, Guid? ProductId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<DiscountRuleDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.discount-rule.read" };
}
