using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Infrastructure.Persistence;

namespace FusionOS.Modules.Crm.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CrmDbContext _context;

    public UnitOfWork(CrmDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
