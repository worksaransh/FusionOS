using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Queries.ListRecurringJournalEntryTemplates;

public sealed class ListRecurringJournalEntryTemplatesQueryHandler : IRequestHandler<ListRecurringJournalEntryTemplatesQuery, PagedResult<RecurringJournalEntryTemplateDto>>
{
    private readonly IRecurringJournalEntryTemplateRepository _repository;

    public ListRecurringJournalEntryTemplatesQueryHandler(IRecurringJournalEntryTemplateRepository repository) => _repository = repository;

    public async Task<PagedResult<RecurringJournalEntryTemplateDto>> Handle(ListRecurringJournalEntryTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _repository.ListAsync(request.CompanyId, request.DueNow, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.DueNow, cancellationToken);

        var dtos = templates.Select(RecurringJournalEntryTemplateMapper.ToDto).ToList();

        return new PagedResult<RecurringJournalEntryTemplateDto>(dtos, request.Page, request.PageSize, total);
    }
}
