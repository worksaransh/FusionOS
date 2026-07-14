using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Infrastructure.Persistence;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CoreDbContext _context;

    public UnitOfWork(CoreDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
