namespace FusionOS.Modules.Hrms.Application.Employees.Contracts;

/// <summary>
/// HRMS's canonical unit-of-work abstraction — one per module, same convention as
/// Finance (Accounts.Contracts) / Manufacturing (BillOfMaterials.Contracts) / CRM
/// (Leads.Contracts) / Quality (Inspections.Contracts) / Maintenance (Assets.Contracts).
/// Every HRMS command handler imports this exact namespace for IUnitOfWork; the concrete
/// implementation lives in Infrastructure over HrmsDbContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
