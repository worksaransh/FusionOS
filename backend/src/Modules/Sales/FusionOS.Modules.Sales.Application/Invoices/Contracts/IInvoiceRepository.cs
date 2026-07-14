namespace FusionOS.Modules.Sales.Application.Invoices.Contracts;

public interface IInvoiceRepository
{
    Task<Domain.Invoices.Invoice?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Invoices.Invoice invoice, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Invoices.Invoice>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
