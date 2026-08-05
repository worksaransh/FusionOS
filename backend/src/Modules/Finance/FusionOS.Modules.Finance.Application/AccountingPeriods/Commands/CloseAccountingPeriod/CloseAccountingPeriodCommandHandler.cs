using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.CloseAccountingPeriod;

public sealed class CloseAccountingPeriodCommandHandler : IRequestHandler<CloseAccountingPeriodCommand, AccountingPeriodDto>
{
    private readonly IAccountingPeriodRepository _repository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CloseAccountingPeriodCommandHandler(
        IAccountingPeriodRepository repository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountingPeriodDto> Handle(CloseAccountingPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _repository.GetByIdAsync(request.CompanyId, request.AccountingPeriodId, cancellationToken)
            ?? throw new KeyNotFoundException($"Accounting period '{request.AccountingPeriodId}' was not found.");

        // Phase 10/11 hardening fix: a Draft entry left inside this period would become
        // permanently unpostable once the period closes (PostJournalEntryCommandHandler's own
        // closed-period check would reject it forever, with no recovery short of reopening the
        // whole period) — block the close instead so the Draft gets resolved first.
        var hasOutstandingDrafts = await _journalEntryRepository.ExistsDraftEntryInDateRangeAsync(
            request.CompanyId, period.PeriodStart, period.PeriodEnd, cancellationToken);
        if (hasOutstandingDrafts)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AccountingPeriodId),
                    "Cannot close this period: one or more Draft journal entries fall within its date range. Post or delete them first."),
            });
        }

        period.Close();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountingPeriodMapper.ToDto(period);
    }
}
