namespace FusionOS.Modules.Procurement.Application.Suppliers.Contracts;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
