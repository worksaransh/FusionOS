using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Infrastructure.Persistence;

namespace FusionOS.Modules.Maintenance.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly MaintenanceDbContext _context;

    public UnitOfWork(MaintenanceDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
