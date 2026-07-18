namespace FusionOS.Modules.Warehouse.Application.PickLists.Contracts;

public interface IPickListRepository
{
    Task<Domain.PickLists.PickList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.PickLists.PickList pickList, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.PickLists.PickList>> ListAsync(Guid companyId, Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default);
}
