namespace FusionOS.Modules.Sales.Application.Quotations.Contracts;

public interface IQuotationRepository
{
    Task<Domain.Quotations.Quotation?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Quotations.Quotation quotation, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Quotations.Quotation>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Customer Portal support (2026-07-22): fetches a single quotation scoped
    /// by CustomerId rather than CompanyId - the portal never trusts a
    /// customer-supplied CompanyId, only the CustomerId claim off its own JWT
    /// (see PortalLoginCommandHandler). Returns null if the quotation does not
    /// exist OR belongs to a different customer - the two cases are
    /// deliberately indistinguishable to the caller (same as GetByIdAsync
    /// already does across companies), so a customer can never learn that a
    /// given id exists under someone else's account.
    /// </summary>
    Task<Domain.Quotations.Quotation?> GetByIdForCustomerAsync(Guid customerId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>Customer Portal support (2026-07-22) - see GetByIdForCustomerAsync.</summary>
    Task<IReadOnlyList<Domain.Quotations.Quotation>> ListByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Customer Portal support (2026-07-22) - see GetByIdForCustomerAsync.</summary>
    Task<int> CountByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
