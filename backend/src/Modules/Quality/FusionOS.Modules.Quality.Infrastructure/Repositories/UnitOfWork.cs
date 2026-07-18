using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Infrastructure.Persistence;

namespace FusionOS.Modules.Quality.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly QualityDbContext _context;

    public UnitOfWork(QualityDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
