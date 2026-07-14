using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Infrastructure.Persistence;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly FinanceDbContext _context;

    public UnitOfWork(FinanceDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
