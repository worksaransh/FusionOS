using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Infrastructure.Persistence;

namespace FusionOS.Modules.Manufacturing.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ManufacturingDbContext _context;

    public UnitOfWork(ManufacturingDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
