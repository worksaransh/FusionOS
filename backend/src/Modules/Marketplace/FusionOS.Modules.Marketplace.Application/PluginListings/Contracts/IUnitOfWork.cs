namespace FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;

/// <summary>
/// Marketplace's canonical unit-of-work abstraction — one per module, same convention as
/// every other module (Finance/Manufacturing/CRM/Quality/Maintenance/HRMS/
/// BusinessIntelligence/AI). Every Marketplace command handler imports this exact
/// namespace for IUnitOfWork; the concrete implementation lives in Infrastructure over
/// MarketplaceDbContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
