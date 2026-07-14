namespace FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
