using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.ExchangeRates;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using MediatR;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Commands.RevalueForeignCurrencyBalances;

/// <summary>
/// Real FX revaluation: recomputes every GL account's open balance in
/// <see cref="RevalueForeignCurrencyBalancesCommand.CurrencyCode"/> at the new rate against
/// what those lines were originally posted at (see JournalEntryLine.ForeignCurrencyCode/
/// ForeignAmount), and posts one balancing journal entry for the net delta into
/// <see cref="RevalueForeignCurrencyBalancesCommand.GainLossAccountId"/> — real computed
/// numbers, not the hardcoded zeros this handler previously returned.
///
/// Only journal lines explicitly tagged with a ForeignCurrencyCode/ForeignAmount are
/// eligible — ordinary base-currency lines (the vast majority of this codebase's
/// transactions today, since no AR/AP/Sales/Procurement aggregate is currency-aware yet)
/// are correctly excluded rather than assumed to carry an implicit exchange rate.
/// </summary>
public sealed class RevalueForeignCurrencyBalancesCommandHandler : IRequestHandler<RevalueForeignCurrencyBalancesCommand, RevalueForeignCurrencyBalancesResultDto>
{
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevalueForeignCurrencyBalancesCommandHandler(
        IExchangeRateRepository exchangeRateRepository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _exchangeRateRepository = exchangeRateRepository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RevalueForeignCurrencyBalancesResultDto> Handle(RevalueForeignCurrencyBalancesCommand request, CancellationToken cancellationToken)
    {
        var balances = await _journalEntryRepository.GetOpenForeignCurrencyBalancesByAccountAsync(request.CompanyId, request.CurrencyCode, cancellationToken);

        var lines = new List<JournalEntryLineInput>();
        var totalGainLoss = 0m;
        var accountsProcessed = 0;

        foreach (var (accountId, netForeignAmount, netBaseAmount) in balances)
        {
            if (netForeignAmount == 0)
                continue;

            var revaluedBaseAmount = netForeignAmount * request.NewExchangeRate;
            var delta = revaluedBaseAmount - netBaseAmount;
            if (delta == 0)
                continue;

            accountsProcessed++;
            totalGainLoss += delta;

            var description = $"FX revaluation: {request.CurrencyCode} @ {request.NewExchangeRate} on {request.EffectiveDate:yyyy-MM-dd}";
            lines.Add(delta > 0
                ? new JournalEntryLineInput(accountId, Debit: delta, Credit: 0m, Description: description)
                : new JournalEntryLineInput(accountId, Debit: 0m, Credit: -delta, Description: description));
        }

        if (lines.Count > 0)
        {
            var gainLossDescription = $"FX revaluation gain/loss: {request.CurrencyCode} @ {request.NewExchangeRate}";
            lines.Add(totalGainLoss > 0
                ? new JournalEntryLineInput(request.GainLossAccountId, Debit: 0m, Credit: totalGainLoss, Description: gainLossDescription)
                : new JournalEntryLineInput(request.GainLossAccountId, Debit: -totalGainLoss, Credit: 0m, Description: gainLossDescription));

            var journalEntry = JournalEntry.Create(
                request.CompanyId,
                $"FX-REVAL-{request.CurrencyCode}-{request.EffectiveDate:yyyyMMdd}",
                lines);
            journalEntry.Post();

            await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
        }

        var exchangeRate = ExchangeRate.Create(
            request.CompanyId,
            request.CurrencyCode,
            request.BaseCurrencyCode,
            request.NewExchangeRate,
            request.EffectiveDate);
        await _exchangeRateRepository.AddAsync(exchangeRate, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RevalueForeignCurrencyBalancesResultDto(
            request.CompanyId,
            request.CurrencyCode,
            RevaluedAmount: Math.Abs(totalGainLoss),
            GainLossAmount: totalGainLoss,
            AccountsProcessed: accountsProcessed,
            ProcessedAt: DateTimeOffset.UtcNow);
    }
}
