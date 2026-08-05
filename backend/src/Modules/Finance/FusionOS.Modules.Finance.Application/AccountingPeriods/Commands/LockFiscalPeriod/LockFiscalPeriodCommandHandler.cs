using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.LockFiscalPeriod;

public sealed class LockFiscalPeriodCommandHandler : IRequestHandler<LockFiscalPeriodCommand, AccountingPeriodDto>
{
    private readonly IAccountingPeriodRepository _repository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LockFiscalPeriodCommandHandler(
        IAccountingPeriodRepository repository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountingPeriodDto> Handle(LockFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _repository.GetByIdAsync(request.CompanyId, request.AccountingPeriodId, cancellationToken)
            ?? throw new KeyNotFoundException($"Accounting period '{request.AccountingPeriodId}' was not found.");

        var hasOutstandingDrafts = await _journalEntryRepository.ExistsDraftEntryInDateRangeAsync(
            request.CompanyId, period.PeriodStart, period.PeriodEnd, cancellationToken);

        if (hasOutstandingDrafts)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AccountingPeriodId),
                    "Cannot lock this fiscal period: one or more Draft journal entries exist in this period."),
            });
        }

        period.Close();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountingPeriodMapper.ToDto(period);
    }
}
