namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

/// <summary>
/// Manufacturing's canonical unit-of-work abstraction — one per module, same convention
/// as Finance (Accounts.Contracts) / Inventory (Products.Contracts). Every Manufacturing
/// command handler imports this exact namespace for IUnitOfWork; the concrete
/// implementation lives in Infrastructure over ManufacturingDbContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
