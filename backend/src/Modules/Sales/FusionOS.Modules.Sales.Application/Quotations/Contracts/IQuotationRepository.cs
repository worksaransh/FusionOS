namespace FusionOS.Modules.Sales.Application.Quotations.Contracts;

public interface IQuotationRepository
{
    Task<Domain.Quotations.Quotation?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Quotations.Quotation quotation, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Quotations.Quotation>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
