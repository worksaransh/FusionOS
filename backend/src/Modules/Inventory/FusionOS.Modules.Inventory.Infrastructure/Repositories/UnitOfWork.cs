using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;

namespace FusionOS.Modules.Inventory.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly InventoryDbContext _context;

    public UnitOfWork(InventoryDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
