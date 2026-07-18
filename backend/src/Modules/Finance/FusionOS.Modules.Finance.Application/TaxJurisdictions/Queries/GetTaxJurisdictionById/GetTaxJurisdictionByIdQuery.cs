using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Queries.GetTaxJurisdictionById;

public sealed record GetTaxJurisdictionByIdQuery(Guid CompanyId, Guid TaxJurisdictionId)
    : IQuery<TaxJurisdictionDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.tax-jurisdiction.read" };
}
