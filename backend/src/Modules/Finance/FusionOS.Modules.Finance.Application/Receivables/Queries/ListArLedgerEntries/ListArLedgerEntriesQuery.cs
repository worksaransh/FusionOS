using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;

namespace FusionOS.Modules.Finance.Application.Receivables.Queries.ListArLedgerEntries;

public sealed record ListArLedgerEntriesQuery(Guid CompanyId, Guid CustomerId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<ArLedgerEntryDto>>;
