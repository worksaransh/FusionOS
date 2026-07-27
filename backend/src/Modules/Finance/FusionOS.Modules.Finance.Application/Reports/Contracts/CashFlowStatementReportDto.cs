namespace FusionOS.Modules.Finance.Application.Reports.Contracts;

/// <summary>
/// One labelled line within a Cash Flow Statement section (Operating/Investing/
/// Financing). Unlike BalanceSheetLineDto/ProfitAndLossLineDto, this is not
/// one-line-per-account — a cash flow line is usually an adjustment or a
/// category total (e.g. "Net income", "Increase in accounts payable",
/// "Acquisition of fixed assets") rather than a single GL account's balance,
/// so it is labelled by a plain string, not an AccountId/AccountCode/AccountName
/// triple.
/// </summary>
public sealed record CashFlowStatementLineDto(string Label, decimal Amount);

/// <summary>
/// Cash Flow Statement for [PeriodStart, PeriodEnd], indirect method (Phase 2
/// closeout follow-up, 2026-07-22) — the standard approach when the figure is
/// derived from GL/ledger balances rather than from a dedicated
/// cash-transaction feed, which is what every other report in this Reports
/// slice already does (Balance Sheet/P&amp;L/Trial Balance are all "canned
/// reports over existing GL data," never a new aggregate).
///
/// <b>What this statement can and cannot honestly claim, given what actually
/// exists in this ledger today (documented here rather than glossed over):</b>
///
/// <list type="bullet">
/// <item>
/// <b>Operating activities</b> = NetIncome (computed the same way
/// GetProfitAndLossReportQueryHandler computes it for the same period — see
/// GetCashFlowStatementReportQueryHandler's own doc comment for why this is a
/// parallel computation rather than a call into that handler) plus two
/// working-capital adjustments: the net movement in the AR ledger and the net
/// movement in the AP ledger over the period. Account/AccountType has no
/// "current vs. fixed" or "current vs. long-term" sub-classification beyond
/// the five base types (Asset/Liability/Equity/Revenue/Expense), so a general
/// "change in all current-asset/current-liability GL accounts" adjustment
/// (the textbook indirect-method approach) is not derivable from Account
/// alone. AR and AP are the two working-capital ledgers this codebase actually
/// tracks as ledgers with their own dated entries (ArLedgerEntry/
/// ApLedgerEntry), so those are what this statement adjusts for. Any other
/// working-capital account with real period activity — accrued liabilities,
/// deferred revenue, prepaid expenses, other non-AR/AP current assets or
/// liabilities — is invisible to this computation. If such an account moves
/// during the period, NetChangeInCash below will NOT tie to the company's
/// actual cash movement; that is a real limitation of this ledger's schema,
/// not a bug in this handler.
/// </item>
/// <item>
/// <b>Investing activities</b> = the acquisition cost of every FixedAsset
/// whose AcquisitionDate falls within the period (a real cash outflow, from
/// real data) MINUS nothing for disposals in the period. As of a concurrent
/// 2026-07-22 change, DisposeFixedAssetCommandHandler now accepts
/// DisposalProceeds and can post a real gain/loss JournalEntry — but
/// DisposalProceeds is a command-time input only, never persisted on the
/// FixedAsset aggregate itself (see FixedAsset.cs — Dispose still only flips
/// IsDisposed/DisposedDate), so a later report reading FixedAsset via
/// IFixedAssetRepository (as this handler does) has no way to recover "how
/// much cash did this disposal bring in." The figure does land in the GL as a
/// debit to FinanceSettings.DefaultDisposalProceedsAccountId when that
/// setting is configured and proceeds are nonzero — a future revision of this
/// report could sum that specific account's posted activity for the period
/// instead of reading FixedAsset directly, but that means taking a dependency
/// on FinanceSettings that this handler deliberately does not take yet (kept
/// out to avoid coupling this report to another slice's in-progress
/// disposal-accounting work). Until then, assets disposed in the period are
/// surfaced as an explicit zero-amount informational line so a reader sees
/// disposals happened, rather than silently omitting them — but their true
/// cash effect is not reflected in this report's totals today.
/// </item>
/// <item>
/// <b>Financing activities</b> = the net change in Equity-type account
/// balances during the period only (capital contributions, owner
/// distributions, and any other Equity-account posting). This ledger has no
/// Loan, share-issuance, or dividend aggregate distinct from a generic GL
/// posting, so a financing-activity journal entry cannot be distinguished
/// from any other Equity-account posting by transaction-type tag — there
/// isn't one. This also means financing activity that runs through a
/// Liability account instead of an Equity account (e.g. drawing down or
/// repaying a loan booked to a "Notes Payable" liability account) is NOT
/// captured anywhere in this statement — Liability-account movements outside
/// of the tracked AP ledger are invisible to it, the same way non-AR/AP
/// working-capital accounts are invisible to the Operating section above.
/// Confirmed before writing this: this codebase has no automated period-close
/// process that posts P&amp;L's net income into a Retained Earnings equity
/// account mid-period (no such command/consumer exists), so an Equity-account
/// balance change observed here is not a double-count of NetIncome above —
/// but this assumption would need revisiting if a period-close feature is
/// ever added.
/// </item>
/// <item>
/// <b>NetChangeInCash</b> = the sum of the three section totals above, i.e. a
/// derived total, not an independently-verified movement of a designated
/// "cash" balance. Account has no IsCash/IsBank flag and no mapping to
/// BankAccount, so this handler cannot itself identify which Asset accounts
/// are actually cash/bank accounts to cross-check against. The most
/// defensible choice available without such a flag is what every other
/// indirect-method preparer does when reconciling by hand: trust the sum of
/// the three classified sections. Given the working-capital and financing
/// gaps documented above, this total is only guaranteed to equal the
/// company's real cash movement in the well-behaved case where every
/// Balance-Sheet account that moved during the period is either Cash/Bank,
/// AR, AP, a FixedAsset acquisition, or Equity — any other account with
/// period activity (e.g. a loan liability, an accrued expense) will make this
/// total diverge from actual cash movement. Adding an IsCash-style flag to
/// Account (or a formal mapping to BankAccount) would let a future revision
/// verify this total directly instead of trusting it by construction.
/// </item>
/// </list>
/// </summary>
public sealed record CashFlowStatementReportDto(
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    decimal NetIncome,
    IReadOnlyList<CashFlowStatementLineDto> OperatingLines,
    decimal NetCashFromOperatingActivities,
    IReadOnlyList<CashFlowStatementLineDto> InvestingLines,
    decimal NetCashFromInvestingActivities,
    IReadOnlyList<CashFlowStatementLineDto> FinancingLines,
    decimal NetCashFromFinancingActivities,
    decimal NetChangeInCash);
