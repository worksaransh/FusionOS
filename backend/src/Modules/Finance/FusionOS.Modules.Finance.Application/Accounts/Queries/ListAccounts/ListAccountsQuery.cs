using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;

namespace FusionOS.Modules.Finance.Application.Accounts.Queries.ListAccounts;

/// <summary>
/// Read-gated (2026-07-14 sprint audit): previously any authenticated user in
/// the tenant could list every account regardless of role — 0 of 17 queries
/// repo-wide implemented IRequirePermission. TenantIsolationBehavior still
/// scopes this to the caller's own CompanyId; this adds the missing
/// role-based gate on top of that.
/// </summary>
public sealed record ListAccountsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<AccountDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.account.read" };
}
