namespace FusionOS.Modules.Maintenance.Application.Assets.Contracts;

/// <summary>
/// Maintenance's canonical unit-of-work abstraction — one per module, same convention as
/// Finance (Accounts.Contracts) / Manufacturing (BillOfMaterials.Contracts) / CRM
/// (Leads.Contracts) / Quality (Inspections.Contracts). Every Maintenance command handler
/// imports this exact namespace for IUnitOfWork; the concrete implementation lives in
/// Infrastructure over MaintenanceDbContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
