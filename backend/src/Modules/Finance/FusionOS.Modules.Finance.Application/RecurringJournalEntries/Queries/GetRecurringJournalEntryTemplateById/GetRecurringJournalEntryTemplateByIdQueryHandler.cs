using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Queries.GetRecurringJournalEntryTemplateById;

public sealed class GetRecurringJournalEntryTemplateByIdQueryHandler : IRequestHandler<GetRecurringJournalEntryTemplateByIdQuery, RecurringJournalEntryTemplateDto>
{
    private readonly IRecurringJournalEntryTemplateRepository _repository;

    public GetRecurringJournalEntryTemplateByIdQueryHandler(IRecurringJournalEntryTemplateRepository repository) => _repository = repository;

    public async Task<RecurringJournalEntryTemplateDto> Handle(GetRecurringJournalEntryTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.CompanyId, request.RecurringJournalEntryTemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Recurring journal entry template '{request.RecurringJournalEntryTemplateId}' was not found.");

        return RecurringJournalEntryTemplateMapper.ToDto(template);
    }
}
