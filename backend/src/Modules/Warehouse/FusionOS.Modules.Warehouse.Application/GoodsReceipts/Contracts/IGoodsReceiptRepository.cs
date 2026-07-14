namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;

public interface IGoodsReceiptRepository
{
    Task<bool> ZoneExistsAsync(Guid companyId, Guid warehouseId, Guid zoneId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.GoodsReceipts.GoodsReceipt receipt, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.GoodsReceipts.GoodsReceipt>> ListAsync(Guid companyId, Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default);
}
