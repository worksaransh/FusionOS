namespace FusionOS.Modules.Quality.Application.Inspections.Contracts;

/// <summary>
/// Quality's canonical unit-of-work abstraction — one per module, same convention as
/// Finance (Accounts.Contracts) / Manufacturing (BillOfMaterials.Contracts) / CRM
/// (Leads.Contracts). Every Quality command handler imports this exact namespace for
/// IUnitOfWork; the concrete implementation lives in Infrastructure over QualityDbContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
