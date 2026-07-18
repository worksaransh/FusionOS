using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;

namespace FusionOS.Modules.Finance.Application.JournalEntries.Queries.ListJournalEntries;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListJournalEntriesQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<JournalEntryDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.journal-entry.read" };
}
