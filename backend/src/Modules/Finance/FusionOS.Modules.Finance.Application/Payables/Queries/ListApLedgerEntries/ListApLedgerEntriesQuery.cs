using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Payables.Contracts;

namespace FusionOS.Modules.Finance.Application.Payables.Queries.ListApLedgerEntries;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListApLedgerEntriesQuery(Guid CompanyId, Guid SupplierId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ApLedgerEntryDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.payable.read" };
}
