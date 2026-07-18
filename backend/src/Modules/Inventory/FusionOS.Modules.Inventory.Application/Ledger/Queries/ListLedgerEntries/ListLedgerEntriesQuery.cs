using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;

namespace FusionOS.Modules.Inventory.Application.Ledger.Queries.ListLedgerEntries;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListLedgerEntriesQuery(Guid CompanyId, Guid ProductId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<InventoryLedgerEntryDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.stock.read" };
}
