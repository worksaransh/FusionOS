using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using FusionOS.Modules.Ai.Infrastructure.Persistence;

namespace FusionOS.Modules.Ai.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AiDbContext _context;

    public UnitOfWork(AiDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
