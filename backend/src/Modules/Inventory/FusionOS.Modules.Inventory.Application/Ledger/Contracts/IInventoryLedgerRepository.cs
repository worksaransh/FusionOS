namespace FusionOS.Modules.Inventory.Application.Ledger.Contracts;

public interface IInventoryLedgerRepository
{
    Task AddAsync(Domain.Ledger.InventoryLedgerEntry entry, CancellationToken cancellationToken = default);

    Task<decimal> SumQuantityAsync(Guid companyId, Guid productId, Guid? warehouseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Ledger.InventoryLedgerEntry>> ListAsync(Guid companyId, Guid productId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);
}
