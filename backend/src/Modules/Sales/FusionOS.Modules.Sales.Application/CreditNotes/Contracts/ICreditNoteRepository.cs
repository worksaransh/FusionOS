namespace FusionOS.Modules.Sales.Application.CreditNotes.Contracts;

public interface ICreditNoteRepository
{
    Task<Domain.CreditNotes.CreditNote?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.CreditNotes.CreditNote creditNote, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.CreditNotes.CreditNote>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sums the quantity already credited for one product across every existing
    /// credit note against this invoice, regardless of Draft/Issued status — the
    /// same in-memory-sum-across-all-statuses shape as
    /// IInvoiceRepository.GetInvoicedQuantityAsync, so CreateCreditNoteCommandHandler
    /// can reject any line that would credit more of a product than the invoice
    /// actually billed for it.
    /// </summary>
    Task<decimal> GetCreditedQuantityAsync(Guid companyId, Guid invoiceId, Guid productId, CancellationToken cancellationToken = default);
}
