namespace FusionOS.Modules.Sales.Application.Invoices.Contracts;

public interface IInvoiceRepository
{
    Task<Domain.Invoices.Invoice?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Invoices.Invoice invoice, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Invoices.Invoice>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sums the quantity already invoiced for one product across every existing
    /// invoice against this sales order (2026-07-14 coverage-audit follow-up:
    /// CreateInvoiceCommandHandler previously fetched the sales order only to
    /// check it existed, never to bound how much of it could be invoiced).
    /// Counting rule (2026-07-20): Draft and Issued invoices BOTH count -
    /// InvoiceStatus has no cancelled/voided state today, so every persisted
    /// invoice is a live claim on the ordered quantity. If a cancellation status
    /// is ever added to InvoiceStatus, this sum must exclude it.
    /// </summary>
    Task<decimal> GetInvoicedQuantityAsync(Guid companyId, Guid salesOrderId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch counterpart of GetInvoicedQuantityAsync (Phase 10 audit fix,
    /// 2026-07-27): CreateInvoiceCommandHandler previously called
    /// GetInvoicedQuantityAsync once per distinct product in the request
    /// (an N+1 round trip per GroupBy iteration). Resolves every distinct
    /// product's already-invoiced quantity for this sales order in one
    /// query - same "batch-resolve by id set" convention as
    /// Crm's ITagRepository.GetByIdsAsync / Finance's ITaxRateRepository.GetByIdsAsync.
    /// Products with no invoiced quantity yet are simply absent from the
    /// result - callers should default to 0.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetInvoicedQuantitiesAsync(Guid companyId, Guid salesOrderId, IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Total invoiced (not just ordered) revenue per salesperson, summed across
    /// every Issued invoice with a non-null SalesPersonId, for the sales
    /// commission summary report (docs/IMPLEMENTATION_PLAN.md Phase 10 item 11).
    /// Draft invoices are excluded — commission is earned on revenue actually
    /// billed, not merely drafted.
    /// </summary>
    Task<IReadOnlyList<(Guid SalesPersonId, decimal TotalInvoicedRevenue)>> GetIssuedInvoiceTotalsBySalesPersonAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Customer Portal support (2026-07-22) - same CustomerId-scoped shape as IQuotationRepository.ListByCustomerAsync; see its doc comment.</summary>
    Task<IReadOnlyList<Domain.Invoices.Invoice>> ListByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Customer Portal support (2026-07-22) - see ListByCustomerAsync.</summary>
    Task<int> CountByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
