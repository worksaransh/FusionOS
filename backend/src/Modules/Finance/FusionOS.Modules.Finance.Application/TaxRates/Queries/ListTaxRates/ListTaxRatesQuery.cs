using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxRates.Queries.ListTaxRates;

public sealed record ListTaxRatesQuery(Guid CompanyId, Guid TaxJurisdictionId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<TaxRateDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.tax-rate.read" };
}
