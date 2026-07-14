using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;

namespace FusionOS.Modules.Finance.Application.JournalEntries.Queries.ListJournalEntries;

public sealed record ListJournalEntriesQuery(Guid CompanyId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<JournalEntryDto>>;
