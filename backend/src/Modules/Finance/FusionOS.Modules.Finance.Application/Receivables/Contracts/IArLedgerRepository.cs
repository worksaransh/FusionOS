namespace FusionOS.Modules.Finance.Application.Receivables.Contracts;

public interface IArLedgerRepository
{
    Task AddAsync(Domain.Receivables.ArLedgerEntry entry, CancellationToken cancellationToken = default);

    Task<decimal> SumAmountAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>Net of every charge/payment entry recorded against one specific invoice — used by RecordPaymentCommandHandler to reject a payment that would overpay that invoice.</summary>
    Task<decimal> SumAmountByInvoiceAsync(Guid companyId, Guid invoiceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Receivables.ArLedgerEntry>> ListAsync(Guid companyId, Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// One row per invoice that has ever had a ledger entry, net of every
    /// charge/payment recorded against it, with rows whose net balance is
    /// zero (fully paid) excluded — used by the AR aging report (Phase M6,
    /// 2026-07-15). ChargeDate is the earliest entry recorded for that
    /// invoice (its original RecordInvoiceCharge), since Finance has no
    /// cross-module FK to Sales' Invoice.InvoiceDate (03_SYSTEM_ARCHITECTURE.md
    /// §2) — this is an approximation of "days outstanding" computed entirely
    /// from Finance's own data, not a fabricated due date.
    /// </summary>
    Task<IReadOnlyList<(Guid CustomerId, Guid InvoiceId, decimal Balance, DateTimeOffset ChargeDate)>> GetOutstandingInvoiceBalancesAsync(Guid companyId, CancellationToken cancellationToken = default);
}
