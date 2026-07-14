namespace FusionOS.Modules.Sales.Application.Customers.Contracts;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
