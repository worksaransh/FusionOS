using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Companies.Contracts;

namespace FusionOS.Modules.Core.Application.Companies.Queries.ListCompanies;

/// <summary>Paginated list per 08_API_STANDARDS.md §4.</summary>
public sealed record ListCompaniesQuery(int Page = 1, int PageSize = 25) : IQuery<PagedResult<CompanyDto>>;

public sealed record PagedResult<T>(IReadOnlyList<T> Data, int Page, int PageSize, int TotalCount);
