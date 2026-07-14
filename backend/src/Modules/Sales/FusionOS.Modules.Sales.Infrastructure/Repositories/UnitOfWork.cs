using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Infrastructure.Persistence;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SalesDbContext _context;

    public UnitOfWork(SalesDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
