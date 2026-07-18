namespace FusionOS.Modules.Finance.Application.Reports.Contracts;

/// <summary>
/// One supplier's outstanding balance, bucketed by days since its oldest
/// recorded charge entry (Phase 2 closeout, 2026-07-18). Grouped per-supplier
/// rather than per-bill — see IApLedgerRepository.GetOutstandingSupplierBalancesAsync
/// for why AP has no reliable per-bill grouping key the way AR's aging report
/// has per-invoice.
/// </summary>
public sealed record ApAgingLineDto(Guid SupplierId, decimal Balance, DateTimeOffset OldestChargeDate, int DaysOutstanding, string Bucket);

/// <summary>
/// Accounts-payable aging summary: every supplier's outstanding balance,
/// grouped into the standard 0-30 / 31-60 / 61-90 / 90+ buckets by days
/// since the oldest recorded charge. Mirrors ArAgingReportDto.
/// </summary>
public sealed record ApAgingReportDto(
    IReadOnlyList<ApAgingLineDto> Lines,
    decimal Bucket0To30Total,
    decimal Bucket31To60Total,
    decimal Bucket61To90Total,
    decimal Bucket90PlusTotal,
    decimal GrandTotal);
