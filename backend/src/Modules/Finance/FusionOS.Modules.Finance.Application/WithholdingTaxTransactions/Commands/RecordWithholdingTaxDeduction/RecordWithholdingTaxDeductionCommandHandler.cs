using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Contracts;
using FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using MediatR;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Commands.RecordWithholdingTaxDeduction;

/// <summary>
/// Computes a real TDS deduction (DeductedAmount = GrossAmount * rate.Percentage / 100) and
/// posts the correct double-entry GL consequence of withholding tax from a payment: the
/// payable/expense side is still debited for the full gross amount it always would be, but
/// part of that amount is credited to a Withholding Tax Payable account (money owed to the
/// government) instead of Bank/Cash — this command posts exactly that reallocating entry,
/// not the payment itself (RecordPaymentCommandHandler on the Payables side still posts the
/// actual cash movement for the net amount, unchanged by this slice).
///
/// Scoped to the TDS-on-payments-to-suppliers direction — TCS-on-receivables (the reverse:
/// a customer withholding tax from a payment to you) needs its own asset-side GL treatment
/// (a Withholding Tax Receivable claimable as a tax credit) and is a documented follow-up,
/// not covered by this command.
/// </summary>
public sealed class RecordWithholdingTaxDeductionCommandHandler : IRequestHandler<RecordWithholdingTaxDeductionCommand, WithholdingTaxTransactionDto>
{
    private readonly IWithholdingTaxRateRepository _rateRepository;
    private readonly IWithholdingTaxTransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordWithholdingTaxDeductionCommandHandler(
        IWithholdingTaxRateRepository rateRepository,
        IWithholdingTaxTransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _rateRepository = rateRepository;
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WithholdingTaxTransactionDto> Handle(RecordWithholdingTaxDeductionCommand request, CancellationToken cancellationToken)
    {
        var rate = await _rateRepository.GetByIdAsync(request.CompanyId, request.WithholdingTaxRateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Withholding tax rate '{request.WithholdingTaxRateId}' was not found.");

        if (!rate.IsActive)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.WithholdingTaxRateId), "This withholding tax rate is deactivated."),
            });
        }

        await EnsureAccountExistsAsync(request.CompanyId, request.PayableGlAccountId, nameof(request.PayableGlAccountId), cancellationToken);
        await EnsureAccountExistsAsync(request.CompanyId, request.WithholdingPayableGlAccountId, nameof(request.WithholdingPayableGlAccountId), cancellationToken);

        var deductedAmount = Math.Round(request.GrossAmount * rate.Percentage / 100m, 2);

        var journalEntry = JournalEntry.Create(
            request.CompanyId,
            $"TDS-{rate.SectionCode}-{request.ReferenceId}",
            new[]
            {
                new JournalEntryLineInput(request.PayableGlAccountId, Debit: deductedAmount, Credit: 0m, Description: $"TDS withheld under {rate.SectionCode}"),
                new JournalEntryLineInput(request.WithholdingPayableGlAccountId, Debit: 0m, Credit: deductedAmount, Description: $"TDS payable under {rate.SectionCode}"),
            });
        journalEntry.Post();
        await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);

        var transaction = Domain.WithholdingTaxTransactions.WithholdingTaxTransaction.Create(
            request.CompanyId, request.WithholdingTaxRateId, request.ReferenceType, request.ReferenceId, request.GrossAmount, deductedAmount);
        await _transactionRepository.AddAsync(transaction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WithholdingTaxTransactionDto(
            transaction.Id, transaction.WithholdingTaxRateId, transaction.ReferenceType, transaction.ReferenceId,
            transaction.GrossAmount, transaction.DeductedAmount, transaction.TransactionDate);
    }

    private async Task EnsureAccountExistsAsync(Guid companyId, Guid accountId, string property, CancellationToken cancellationToken)
    {
        if (!await _accountRepository.ExistsAsync(companyId, accountId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(property, $"Account '{accountId}' does not exist for this company."),
            });
        }
    }
}
