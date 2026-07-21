namespace FusionOS.Modules.Finance.Application.Payables.Contracts;

/// <summary>
/// Mirrors <see cref="Receivables.Contracts.IArLedgerRepository"/>, scoped to
/// Supplier rather than Customer. Deliberately smaller than the AR contract:
/// AR needs a per-invoice sum (SumAmountByInvoiceAsync) because its
/// RecordPaymentCommandHandler guards a payment against one specific
/// invoice's balance; AP's equivalent guard is scoped to the supplier's total
/// outstanding balance instead (see ApLedgerEntry's class doc comment for
/// why), so SumAmountAsync alone serves both GetSupplierBalanceQueryHandler
/// and RecordPaymentCommandHandler — no separate by-purchase-order sum
/// method is needed for this slice. AR's GetOutstandingInvoiceBalancesAsync
/// (backing the AR aging report, Phase M6) also has no AP equivalent here —
/// an AP aging report wasn't asked for in this slice.
/// </summary>
public interface IApLedgerRepository
{
    Task AddAsync(Domain.Payables.ApLedgerEntry entry, CancellationToken cancellationToken = default);

    Task<decimal> SumAmountAsync(Guid companyId, Guid supplierId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Total already charged against one specific purchase order (three-way
    /// match, 2026-07-20) - unlike SumAmountAsync's supplier-wide scope, this
    /// is the "already billed" figure RecordBillChargeCommandHandler compares
    /// against PurchaseOrderFact's OrderedAmount/ReceivedAmount ceilings before
    /// accepting a new manual charge for the same PurchaseOrderId.
    /// </summary>
    Task<decimal> SumAmountByPurchaseOrderAsync(Guid companyId, Guid purchaseOrderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Payables.ApLedgerEntry>> ListAsync(Guid companyId, Guid supplierId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, Guid supplierId, CancellationToken cancellationToken = default);

    /// <summary>
    /// One row per supplier with a nonzero net balance, for the AP aging report
    /// (Phase 2 closeout, 2026-07-18). Grouped by SupplierId rather than by
    /// PurchaseOrderId the way AR's equivalent groups by InvoiceId — unlike
    /// ArLedgerEntry.InvoiceId (mandatory, one row per invoice),
    /// ApLedgerEntry.PurchaseOrderId is optional (an ad-hoc bill has none), so
    /// it isn't a reliable "one row per bill" grouping key here; supplier is
    /// the only key every entry always has. OldestChargeDate is the earliest
    /// entry recorded for that supplier, same "days outstanding computed
    /// entirely from Finance's own data" approximation AR's aging report uses.
    /// </summary>
    Task<IReadOnlyList<(Guid SupplierId, decimal Balance, DateTimeOffset OldestChargeDate)>> GetOutstandingSupplierBalancesAsync(Guid companyId, CancellationToken cancellationToken = default);
}
