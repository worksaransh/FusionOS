namespace FusionOS.Modules.Finance.Application.Accounts.Contracts;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
