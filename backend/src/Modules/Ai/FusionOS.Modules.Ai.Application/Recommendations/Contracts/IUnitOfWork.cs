namespace FusionOS.Modules.Ai.Application.Recommendations.Contracts;

/// <summary>
/// AI's canonical unit-of-work abstraction — one per module, same convention as every
/// other module (Finance/Manufacturing/CRM/Quality/Maintenance/HRMS/BusinessIntelligence).
/// Every AI command handler imports this exact namespace for IUnitOfWork; the concrete
/// implementation lives in Infrastructure over AiDbContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
