using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Infrastructure.Persistence;

namespace FusionOS.Modules.Warehouse.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly WarehouseDbContext _context;

    public UnitOfWork(WarehouseDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
