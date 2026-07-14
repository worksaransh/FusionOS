using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;

namespace FusionOS.Modules.Inventory.Application.Ledger.Queries.ListLedgerEntries;

public sealed record ListLedgerEntriesQuery(Guid CompanyId, Guid ProductId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<InventoryLedgerEntryDto>>;
