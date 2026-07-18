namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;

/// <summary>
/// IntegrationHub's canonical unit-of-work abstraction — one per module, same convention
/// as every other module (Finance/Manufacturing/CRM/Quality/Maintenance/HRMS/
/// BusinessIntelligence/AI/Marketplace). Every IntegrationHub command handler imports this
/// exact namespace for IUnitOfWork; the concrete implementation lives in Infrastructure
/// over IntegrationHubDbContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
