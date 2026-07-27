using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Contracts;
using FusionOS.Modules.Finance.Domain.Accounts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetCashFlowStatementReport;

/// <summary>
/// Indirect-method Cash Flow Statement handler (2026-07-22) — see
/// CashFlowStatementReportDto's own doc comment for the full, honest
/// breakdown of what each section can and cannot claim given this ledger's
/// actual schema. Depends on the same five repositories every other Finance
/// slice already composes from (IJournalEntryRepository/IAccountRepository
/// for the GL fold, IArLedgerRepository/IApLedgerRepository for the two
/// working-capital ledgers, IFixedAssetRepository for investing activity) —
/// no new repository interface, no new aggregate.
///
/// The Net Income computation below deliberately duplicates
/// GetProfitAndLossReportQueryHandler's own Revenue/Expense fold instead of
/// calling that handler via ISender: no handler in this Reports slice invokes
/// another handler (Balance Sheet and P&amp;L each independently fold
/// GetPostedBalancesByAccountInRangeAsync/...AsOfAsync themselves), and
/// introducing that cross-handler call here would be a new pattern for this
/// slice, not a reuse of an existing one — a small, well-contained
/// duplication of a five-line fold is the smaller change.
/// </summary>
public sealed class GetCashFlowStatementReportQueryHandler : IRequestHandler<GetCashFlowStatementReportQuery, CashFlowStatementReportDto>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IArLedgerRepository _arLedgerRepository;
    private readonly IApLedgerRepository _apLedgerRepository;
    private readonly IFixedAssetRepository _fixedAssetRepository;

    public GetCashFlowStatementReportQueryHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        IArLedgerRepository arLedgerRepository,
        IApLedgerRepository apLedgerRepository,
        IFixedAssetRepository fixedAssetRepository)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _arLedgerRepository = arLedgerRepository;
        _apLedgerRepository = apLedgerRepository;
        _fixedAssetRepository = fixedAssetRepository;
    }

    public async Task<CashFlowStatementReportDto> Handle(GetCashFlowStatementReportQuery request, CancellationToken cancellationToken)
    {
        var balances = await _journalEntryRepository.GetPostedBalancesByAccountInRangeAsync(request.CompanyId, request.PeriodStart, request.PeriodEnd, cancellationToken);
        var accounts = (await _accountRepository.ListAllAsync(request.CompanyId, cancellationToken)).ToDictionary(a => a.Id);

        // --- Net income: same fold GetProfitAndLossReportQueryHandler performs for the period (see class doc comment on why this isn't a call into that handler). ---
        decimal totalRevenue = 0m;
        decimal totalExpenses = 0m;
        decimal totalEquityMovement = 0m;

        foreach (var b in balances)
        {
            if (!accounts.TryGetValue(b.AccountId, out var account))
                continue; // an account referenced only by a posted line that predates a since-corrected chart is not this report's concern to repair (same exclusion Balance Sheet/P&L apply)

            switch (account.AccountType)
            {
                case AccountType.Revenue:
                    totalRevenue += b.TotalCredit - b.TotalDebit;
                    break;
                case AccountType.Expense:
                    totalExpenses += b.TotalDebit - b.TotalCredit;
                    break;
                case AccountType.Equity:
                    // Financing activities: net change in Equity-account balances during the period (see class/DTO doc comment for the honest limitations of this proxy).
                    totalEquityMovement += b.TotalCredit - b.TotalDebit;
                    break;
            }
        }

        var netIncome = totalRevenue - totalExpenses;

        // --- Operating activities: net income plus AR/AP working-capital movement for the period. ---
        var arMovement = await _arLedgerRepository.SumAmountInRangeAsync(request.CompanyId, request.PeriodStart, request.PeriodEnd, cancellationToken);
        var apMovement = await _apLedgerRepository.SumAmountInRangeAsync(request.CompanyId, request.PeriodStart, request.PeriodEnd, cancellationToken);

        var changeInAr = -arMovement; // AR growing during the period means revenue was recognized but not yet collected in cash
        var changeInAp = apMovement;  // AP growing during the period means an expense/bill was recognized but not yet paid in cash

        var operatingLines = new List<CashFlowStatementLineDto>
        {
            new("Net income", netIncome),
            new("Change in accounts receivable", changeInAr),
            new("Change in accounts payable", changeInAp),
        };
        var netCashFromOperating = netIncome + changeInAr + changeInAp;

        // --- Investing activities: fixed assets acquired in the period (real cash outflow); disposals noted but not valued (see DTO doc comment). ---
        var fixedAssets = await _fixedAssetRepository.ListAllAsync(request.CompanyId, cancellationToken);

        var acquiredInPeriod = fixedAssets
            .Where(a => a.AcquisitionDate >= request.PeriodStart && a.AcquisitionDate <= request.PeriodEnd)
            .ToList();
        var disposedInPeriod = fixedAssets
            .Where(a => a.IsDisposed && a.DisposedDate.HasValue && a.DisposedDate.Value >= request.PeriodStart && a.DisposedDate.Value <= request.PeriodEnd)
            .ToList();

        var acquisitionCash = -acquiredInPeriod.Sum(a => a.AcquisitionCost);

        var investingLines = new List<CashFlowStatementLineDto>
        {
            new("Acquisition of fixed assets", acquisitionCash),
        };
        if (disposedInPeriod.Count > 0)
        {
            // Deliberately zero — DisposalProceeds is a DisposeFixedAssetCommand-time input, never persisted back onto the FixedAsset aggregate (see FixedAsset.cs), so a later reader of IFixedAssetRepository has no way to recover it here. It may separately be posted to the GL under FinanceSettings.DefaultDisposalProceedsAccountId, but this handler doesn't take a dependency on FinanceSettings to chase that (see CashFlowStatementReportDto's doc comment). Surfaced as an explicit line rather than silently omitted.
            investingLines.Add(new CashFlowStatementLineDto($"Disposal of fixed assets ({disposedInPeriod.Count} disposed this period — proceeds not reflected in this report)", 0m));
        }
        var netCashFromInvesting = investingLines.Sum(l => l.Amount);

        // --- Financing activities: net change in Equity-account balances during the period only (see class/DTO doc comment for what this cannot capture, e.g. loan liabilities). ---
        var financingLines = new List<CashFlowStatementLineDto>
        {
            new("Net change in equity", totalEquityMovement),
        };
        var netCashFromFinancing = financingLines.Sum(l => l.Amount);

        var netChangeInCash = netCashFromOperating + netCashFromInvesting + netCashFromFinancing;

        return new CashFlowStatementReportDto(
            request.PeriodStart,
            request.PeriodEnd,
            netIncome,
            operatingLines,
            netCashFromOperating,
            investingLines,
            netCashFromInvesting,
            financingLines,
            netCashFromFinancing,
            netChangeInCash);
    }
}
