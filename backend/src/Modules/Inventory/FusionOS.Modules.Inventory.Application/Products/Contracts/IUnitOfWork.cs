namespace FusionOS.Modules.Inventory.Application.Products.Contracts;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
