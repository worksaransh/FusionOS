using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxRates.Queries.GetTaxRateById;

public sealed record GetTaxRateByIdQuery(Guid CompanyId, Guid TaxRateId)
    : IQuery<TaxRateDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.tax-rate.read" };
}
