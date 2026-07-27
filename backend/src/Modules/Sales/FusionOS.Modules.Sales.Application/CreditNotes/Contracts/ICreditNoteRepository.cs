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

    /// <summary>
    /// Batch counterpart of GetCreditedQuantityAsync (Phase 10 audit fix,
    /// 2026-07-27): CreateCreditNoteCommandHandler previously called
    /// GetCreditedQuantityAsync once per distinct product in the request
    /// (an N+1 round trip per GroupBy iteration). Resolves every distinct
    /// product's already-credited quantity for this invoice in one query -
    /// same "batch-resolve by id set" convention as Crm's
    /// ITagRepository.GetByIdsAsync / Finance's ITaxRateRepository.GetByIdsAsync.
    /// Products with no credited quantity yet are simply absent from the
    /// result - callers should default to 0.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetCreditedQuantitiesAsync(Guid companyId, Guid invoiceId, IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default);
}
