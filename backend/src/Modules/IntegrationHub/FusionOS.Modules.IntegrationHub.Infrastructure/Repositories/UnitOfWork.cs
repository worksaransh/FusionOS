using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IntegrationHubDbContext _context;

    public UnitOfWork(IntegrationHubDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
