namespace FusionOS.Modules.Finance.Application.Reports.Contracts;

/// <summary>
/// One outstanding invoice balance, bucketed by days since its original
/// charge entry (Phase M6, 2026-07-15). See IArLedgerRepository.GetOutstandingInvoiceBalancesAsync
/// for why ChargeDate — not a due date — is the basis for DaysOutstanding.
/// </summary>
public sealed record ArAgingLineDto(Guid CustomerId, Guid InvoiceId, decimal Balance, DateTimeOffset ChargeDate, int DaysOutstanding, string Bucket);

/// <summary>
/// Accounts-receivable aging summary: every outstanding invoice balance,
/// grouped into the standard 0-30 / 31-60 / 61-90 / 90+ buckets by days
/// since charge.
/// </summary>
public sealed record ArAgingReportDto(
    IReadOnlyList<ArAgingLineDto> Lines,
    decimal Bucket0To30Total,
    decimal Bucket31To60Total,
    decimal Bucket61To90Total,
    decimal Bucket90PlusTotal,
    decimal GrandTotal);
