namespace FusionOS.Modules.Crm.Application.Leads.Contracts;

/// <summary>
/// CRM's canonical unit-of-work abstraction — one per module, same convention as
/// Finance (Accounts.Contracts) / Manufacturing (BillOfMaterials.Contracts). Every CRM
/// command handler imports this exact namespace for IUnitOfWork; the concrete
/// implementation lives in Infrastructure over CrmDbContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
