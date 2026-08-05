using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Contracts;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxRates.Queries.ListWithholdingTaxRates;

public sealed record ListWithholdingTaxRatesQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<WithholdingTaxRateDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.withholding-tax-rate.read" };
}
