namespace FusionOS.Modules.Finance.Application.Reports.Contracts;

/// <summary>
/// One account's activity for a P&amp;L period (Phase 2 closeout, 2026-07-18).
/// Amount is normal-balance-signed: for a Revenue account this is (Credit -
/// Debit) — revenue is credit-normal — and for an Expense account this is
/// (Debit - Credit) — expense is debit-normal — so a positive Amount always
/// reads as "revenue earned" or "expense incurred," never a raw signed
/// debit-minus-credit a reader would have to mentally flip per account type.
/// </summary>
public sealed record ProfitAndLossLineDto(Guid AccountId, string AccountCode, string AccountName, decimal Amount);

/// <summary>
/// Profit &amp; Loss (Income Statement) for [PeriodStart, PeriodEnd]: every
/// Revenue and Expense account's Posted activity for the period, split into
/// two lists, plus the totals and NetIncome = TotalRevenue - TotalExpenses.
/// Computed entirely from GetPostedBalancesByAccountInRangeAsync — no new
/// aggregate, same "canned report over existing GL data" shape as Trial
/// Balance/AR Aging.
/// </summary>
public sealed record ProfitAndLossReportDto(
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    IReadOnlyList<ProfitAndLossLineDto> RevenueLines,
    IReadOnlyList<ProfitAndLossLineDto> ExpenseLines,
    decimal TotalRevenue,
    decimal TotalExpenses,
    decimal NetIncome);
