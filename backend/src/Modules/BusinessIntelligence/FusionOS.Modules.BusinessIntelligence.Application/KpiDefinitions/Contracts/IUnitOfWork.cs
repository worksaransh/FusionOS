namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;

/// <summary>
/// Business Intelligence's canonical unit-of-work abstraction — one per module, same
/// convention as every other module (Finance/Manufacturing/CRM/Quality/Maintenance/HRMS).
/// Every BI command handler imports this exact namespace for IUnitOfWork; the concrete
/// implementation lives in Infrastructure over BusinessIntelligenceDbContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
