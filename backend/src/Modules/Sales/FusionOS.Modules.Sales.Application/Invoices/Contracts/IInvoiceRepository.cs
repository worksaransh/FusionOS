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
    /// </summary>
    Task<decimal> GetInvoicedQuantityAsync(Guid companyId, Guid salesOrderId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Total invoiced (not just ordered) revenue per salesperson, summed across
    /// every Issued invoice with a non-null SalesPersonId, for the sales
    /// commission summary report (docs/IMPLEMENTATION_PLAN.md Phase 10 item 11).
    /// Draft invoices are excluded — commission is earned on revenue actually
    /// billed, not merely drafted.
    /// </summary>
    Task<IReadOnlyList<(Guid SalesPersonId, decimal TotalInvoicedRevenue)>> GetIssuedInvoiceTotalsBySalesPersonAsync(Guid companyId, CancellationToken cancellationToken = default);
}
