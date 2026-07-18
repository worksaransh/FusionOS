using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Commissions.Contracts;

namespace FusionOS.Modules.Sales.Application.Commissions.Queries.ListCommissionRates;

public sealed record ListCommissionRatesQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<SalesCommissionRateDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.commission-rate.read" };
}
