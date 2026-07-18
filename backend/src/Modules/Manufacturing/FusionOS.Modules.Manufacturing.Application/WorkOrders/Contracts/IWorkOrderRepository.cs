namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

public interface IWorkOrderRepository
{
    Task<Domain.WorkOrders.WorkOrder?> GetByIdAsync(Guid companyId, Guid workOrderId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.WorkOrders.WorkOrder workOrder, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.WorkOrders.WorkOrder>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
