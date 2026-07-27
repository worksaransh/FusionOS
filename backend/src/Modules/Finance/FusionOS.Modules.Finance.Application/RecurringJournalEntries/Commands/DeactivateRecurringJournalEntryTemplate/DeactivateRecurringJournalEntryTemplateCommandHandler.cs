using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.DeactivateRecurringJournalEntryTemplate;

public sealed class DeactivateRecurringJournalEntryTemplateCommandHandler : IRequestHandler<DeactivateRecurringJournalEntryTemplateCommand, RecurringJournalEntryTemplateDto>
{
    private readonly IRecurringJournalEntryTemplateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateRecurringJournalEntryTemplateCommandHandler(IRecurringJournalEntryTemplateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RecurringJournalEntryTemplateDto> Handle(DeactivateRecurringJournalEntryTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.CompanyId, request.RecurringJournalEntryTemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Recurring journal entry template '{request.RecurringJournalEntryTemplateId}' was not found.");

        template.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RecurringJournalEntryTemplateMapper.ToDto(template);
    }
}
