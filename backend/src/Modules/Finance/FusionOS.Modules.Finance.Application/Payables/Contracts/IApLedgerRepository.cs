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

    Task<IReadOnlyList<Domain.Payables.ApLedgerEntry>> ListAsync(Guid companyId, Guid supplierId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, Guid supplierId, CancellationToken cancellationToken = default);
}
