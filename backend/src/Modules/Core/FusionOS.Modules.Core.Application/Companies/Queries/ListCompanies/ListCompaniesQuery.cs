using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Companies.Contracts;

namespace FusionOS.Modules.Core.Application.Companies.Queries.ListCompanies;

/// <summary>
/// Paginated list per 08_API_STANDARDS.md §4. Read-gated to match its sibling
/// GetCompanyByIdQuery (both require "core.company.read") - previously this
/// list endpoint required no permission at all while the single-record fetch
/// did, an inconsistent read-side gap for equivalent data.
/// </summary>
public sealed record ListCompaniesQuery(int Page = 1, int PageSize = 25) : IQuery<PagedResult<CompanyDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.company.read" };
}

public sealed record PagedResult<T>(IReadOnlyList<T> Data, int Page, int PageSize, int TotalCount);
