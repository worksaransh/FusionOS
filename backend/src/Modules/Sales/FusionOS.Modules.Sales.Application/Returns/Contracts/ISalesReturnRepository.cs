namespace FusionOS.Modules.Sales.Application.Returns.Contracts;

public interface ISalesReturnRepository
{
    Task<Domain.Returns.SalesReturn?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Returns.SalesReturn salesReturn, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Returns.SalesReturn>> ListAsync(Guid companyId, int page, int pageSize, string? status = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? status = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Returns.SalesReturn>> ListByInvoiceAsync(Guid companyId, Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sums the quantity already requested-for-return for one product across every
    /// existing sales return against this invoice, EXCLUDING Rejected returns (a
    /// rejected return never consumed any of the invoice's returnable quantity, so
    /// it must not count against the cap) — same in-memory-sum-across-live-statuses
    /// shape as ICreditNoteRepository.GetCreditedQuantityAsync, so
    /// CreateSalesReturnCommandHandler can reject any line that would return more of
    /// a product than the invoice actually billed for it, net of what's already
    /// pending/approved/completed return against it.
    /// </summary>
    Task<decimal> GetReturnedQuantityAsync(Guid companyId, Guid invoiceId, Guid productId, CancellationToken cancellationToken = default);
}
