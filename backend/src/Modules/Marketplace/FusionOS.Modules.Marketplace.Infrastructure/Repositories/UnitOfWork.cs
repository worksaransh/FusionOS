using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using FusionOS.Modules.Marketplace.Infrastructure.Persistence;

namespace FusionOS.Modules.Marketplace.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly MarketplaceDbContext _context;

    public UnitOfWork(MarketplaceDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
