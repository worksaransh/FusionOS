namespace FusionOS.Modules.Procurement.Application.Rfqs.Contracts;

public interface IRfqRepository
{
    Task<Domain.Rfqs.RequestForQuotation?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Rfqs.RequestForQuotation rfq, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Rfqs.RequestForQuotation>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
