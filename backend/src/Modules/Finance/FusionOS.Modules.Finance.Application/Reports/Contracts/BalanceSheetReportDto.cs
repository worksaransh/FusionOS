namespace FusionOS.Modules.Finance.Application.Reports.Contracts;

/// <summary>
/// One account's as-of balance for a Balance Sheet (Phase 2 closeout,
/// 2026-07-18). Amount is normal-balance-signed: Asset is (Debit - Credit) —
/// debit-normal — Liability/Equity is (Credit - Debit) — credit-normal — so
/// a positive Amount always reads as "what the business has/owes/is worth,"
/// never a raw signed debit-minus-credit a reader would have to flip per
/// account type.
/// </summary>
public sealed record BalanceSheetLineDto(Guid AccountId, string AccountCode, string AccountName, decimal Amount);

/// <summary>
/// Balance Sheet as of a date: every Asset/Liability/Equity account's
/// cumulative Posted balance since inception through AsOfDate, split into
/// three lists, plus totals and IsBalanced (TotalAssets == TotalLiabilities +
/// TotalEquity — the fundamental accounting identity, restated at the report
/// level the same way TrialBalanceReportDto restates double-entry balance).
/// Computed entirely from the existing GetPostedBalancesByAccountAsOfAsync —
/// no new aggregate, same "canned report over existing GL data" shape as
/// Trial Balance/P&amp;L.
/// </summary>
public sealed record BalanceSheetReportDto(
    DateTimeOffset AsOfDate,
    IReadOnlyList<BalanceSheetLineDto> AssetLines,
    IReadOnlyList<BalanceSheetLineDto> LiabilityLines,
    IReadOnlyList<BalanceSheetLineDto> EquityLines,
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal TotalEquity,
    bool IsBalanced);
