namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;

public interface IPurchaseOrderRepository
{
    Task<Domain.PurchaseOrders.PurchaseOrder?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.PurchaseOrders.PurchaseOrder order, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.PurchaseOrders.PurchaseOrder>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
