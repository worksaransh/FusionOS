using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using MediatR;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.GenerateJournalEntryFromTemplate;

/// <summary>
/// <b>How this reuses CreateJournalEntryCommandHandler's validation instead of duplicating
/// it:</b> this handler does not re-implement the balance check, the account/cost-center
/// existence checks, or the AccountingPeriod closed-period guard. It builds a
/// <see cref="CreateJournalEntryCommand"/> from the template's current lines and hands it to a
/// plain, directly-instantiated <see cref="CreateJournalEntryCommandHandler"/> — the exact same
/// class the JournalEntriesController path uses — passing through this handler's own repository
/// dependencies (all four are already required here for other reasons: IJournalEntryRepository
/// to persist the generated entry, IAccountRepository/ICostCenterRepository/
/// IAccountingPeriodRepository purely to satisfy that handler's constructor). Nothing about
/// account existence, cost-center existence, closed-period rejection, or the debit==credit
/// invariant is re-checked or re-implemented here.
///
/// Going through MediatR's ISender to "Send" a CreateJournalEntryCommand from inside another
/// handler was considered but rejected: no Application-layer handler anywhere in this codebase
/// takes an ISender/IMediator dependency (that pattern is reserved for the Api layer's
/// controllers), and introducing a nested MediatR dispatch here would also re-run the
/// authorization/audit pipeline behaviors for a command the caller never issued. Instantiating
/// the collaborator handler directly is a plain object composition — it has no dependencies of
/// its own beyond the repositories already passed in — and keeps the single MediatR request per
/// HTTP call invariant.
///
/// NextDueDate is advanced <i>before</i> calling into CreateJournalEntryCommandHandler.Handle
/// (whose SaveChangesAsync call is the same DbContext instance this handler's IUnitOfWork
/// wraps), so the template's advance is captured in the exact same database round trip as the
/// new JournalEntry — if creation is rejected (e.g. a closed period, or a missing account),
/// nothing is saved and NextDueDate reverts to its persisted value along with everything else.
/// </summary>
public sealed class GenerateJournalEntryFromTemplateCommandHandler : IRequestHandler<GenerateJournalEntryFromTemplateCommand, JournalEntryDto>
{
    private readonly IRecurringJournalEntryTemplateRepository _templateRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateJournalEntryFromTemplateCommandHandler(
        IRecurringJournalEntryTemplateRepository templateRepository,
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        ICostCenterRepository costCenterRepository,
        IAccountingPeriodRepository accountingPeriodRepository,
        IUnitOfWork unitOfWork)
    {
        _templateRepository = templateRepository;
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _costCenterRepository = costCenterRepository;
        _accountingPeriodRepository = accountingPeriodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<JournalEntryDto> Handle(GenerateJournalEntryFromTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(request.CompanyId, request.RecurringJournalEntryTemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Recurring journal entry template '{request.RecurringJournalEntryTemplateId}' was not found.");

        if (!template.IsActive)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.RecurringJournalEntryTemplateId), "Cannot generate a journal entry from a deactivated recurring journal entry template."),
            });
        }

        var lines = template.Lines
            .Select(l => new JournalEntryLineInput(l.AccountId, l.Debit, l.Credit, template.Reference, l.CostCenterId))
            .ToList();

        // Advance first so the change rides along in the same SaveChangesAsync call the
        // delegated handler below issues — see this class's own doc comment.
        template.AdvanceNextDueDate();

        var createHandler = new CreateJournalEntryCommandHandler(_journalEntryRepository, _accountRepository, _costCenterRepository, _accountingPeriodRepository, _unitOfWork);
        var createCommand = new CreateJournalEntryCommand(request.CompanyId, template.Reference, lines);

        return await createHandler.Handle(createCommand, cancellationToken);
    }
}
