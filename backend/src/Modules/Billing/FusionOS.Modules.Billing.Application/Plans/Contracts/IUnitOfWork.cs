namespace FusionOS.Modules.Billing.Application.Plans.Contracts;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
