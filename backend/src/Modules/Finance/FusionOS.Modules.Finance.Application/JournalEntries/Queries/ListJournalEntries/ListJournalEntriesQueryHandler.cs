using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.JournalEntries.Queries.ListJournalEntries;

public sealed class ListJournalEntriesQueryHandler : IRequestHandler<ListJournalEntriesQuery, PagedResult<JournalEntryDto>>
{
    private readonly IJournalEntryRepository _repository;

    public ListJournalEntriesQueryHandler(IJournalEntryRepository repository) => _repository = repository;

    public async Task<PagedResult<JournalEntryDto>> Handle(ListJournalEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = entries.Select(CreateJournalEntryCommandHandler.MapToDto).ToList();

        return new PagedResult<JournalEntryDto>(dtos, request.Page, request.PageSize, total);
    }
}
