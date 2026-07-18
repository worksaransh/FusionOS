using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;

namespace FusionOS.Modules.Finance.Application.Receivables.Queries.ListArLedgerEntries;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListArLedgerEntriesQuery(Guid CompanyId, Guid CustomerId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ArLedgerEntryDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.receivable.read" };
}
