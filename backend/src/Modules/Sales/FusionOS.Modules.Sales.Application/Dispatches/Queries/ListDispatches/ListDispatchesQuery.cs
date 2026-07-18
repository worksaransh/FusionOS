using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;

namespace FusionOS.Modules.Sales.Application.Dispatches.Queries.ListDispatches;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListDispatchesQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<DispatchDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.dispatch.read" };
}
