using FusionOS.Modules.Billing.Application.Plans.Contracts;
using FusionOS.Modules.Billing.Infrastructure.Persistence;

namespace FusionOS.Modules.Billing.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BillingDbContext _context;

    public UnitOfWork(BillingDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
