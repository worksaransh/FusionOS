using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Queries.ListTaxJurisdictions;

public sealed record ListTaxJurisdictionsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<TaxJurisdictionDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.tax-jurisdiction.read" };
}
