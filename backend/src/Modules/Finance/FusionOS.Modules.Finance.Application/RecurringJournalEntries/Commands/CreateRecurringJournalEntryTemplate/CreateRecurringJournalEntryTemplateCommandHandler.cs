using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.RecurringJournalEntries;
using MediatR;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.CreateRecurringJournalEntryTemplate;

/// <summary>Same account/cost-center existence-check split CreateJournalEntryCommandHandler uses — grouped once per distinct id rather than once per line.</summary>
public sealed class CreateRecurringJournalEntryTemplateCommandHandler : IRequestHandler<CreateRecurringJournalEntryTemplateCommand, RecurringJournalEntryTemplateDto>
{
    private readonly IRecurringJournalEntryTemplateRepository _repository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRecurringJournalEntryTemplateCommandHandler(
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

    public async Task<RecurringJournalEntryTemplateDto> Handle(CreateRecurringJournalEntryTemplateCommand request, CancellationToken cancellationToken)
    {
        foreach (var accountLines in request.Lines.GroupBy(l => l.AccountId))
        {
            if (!await _accountRepository.ExistsAsync(request.CompanyId, accountLines.Key, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(RecurringJournalEntryTemplateLineInput.AccountId), $"Account '{accountLines.Key}' does not exist for this company."),
                });
            }
        }

        foreach (var costCenterLines in request.Lines.Where(l => l.CostCenterId is not null).GroupBy(l => l.CostCenterId!.Value))
        {
            if (await _costCenterRepository.GetByIdAsync(request.CompanyId, costCenterLines.Key, cancellationToken) is null)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(RecurringJournalEntryTemplateLineInput.CostCenterId), $"Cost center '{costCenterLines.Key}' does not exist for this company."),
                });
            }
        }

        var template = RecurringJournalEntryTemplate.Create(request.CompanyId, request.Reference, request.Frequency, request.NextDueDate, request.Lines);

        await _repository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RecurringJournalEntryTemplateMapper.ToDto(template);
    }
}
