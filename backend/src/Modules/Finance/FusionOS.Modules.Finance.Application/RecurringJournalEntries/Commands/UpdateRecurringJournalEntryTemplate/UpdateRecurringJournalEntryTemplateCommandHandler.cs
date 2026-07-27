using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.UpdateRecurringJournalEntryTemplate;

public sealed class UpdateRecurringJournalEntryTemplateCommandHandler : IRequestHandler<UpdateRecurringJournalEntryTemplateCommand, RecurringJournalEntryTemplateDto>
{
    private readonly IRecurringJournalEntryTemplateRepository _repository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRecurringJournalEntryTemplateCommandHandler(
        IRecurringJournalEntryTemplateRepository repository,
        IAccountRepository accountRepository,
        ICostCenterRepository costCenterRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _costCenterRepository = costCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RecurringJournalEntryTemplateDto> Handle(UpdateRecurringJournalEntryTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.CompanyId, request.RecurringJournalEntryTemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Recurring journal entry template '{request.RecurringJournalEntryTemplateId}' was not found.");

        foreach (var accountLines in request.Lines.GroupBy(l => l.AccountId))
        {
            if (!await _accountRepository.ExistsAsync(request.CompanyId, accountLines.Key, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(Domain.RecurringJournalEntries.RecurringJournalEntryTemplateLineInput.AccountId), $"Account '{accountLines.Key}' does not exist for this company."),
                });
            }
        }

        foreach (var costCenterLines in request.Lines.Where(l => l.CostCenterId is not null).GroupBy(l => l.CostCenterId!.Value))
        {
            if (await _costCenterRepository.GetByIdAsync(request.CompanyId, costCenterLines.Key, cancellationToken) is null)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(Domain.RecurringJournalEntries.RecurringJournalEntryTemplateLineInput.CostCenterId), $"Cost center '{costCenterLines.Key}' does not exist for this company."),
                });
            }
        }

        template.UpdateDetails(request.Reference, request.Frequency, request.NextDueDate, request.Lines);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RecurringJournalEntryTemplateMapper.ToDto(template);
    }
}
