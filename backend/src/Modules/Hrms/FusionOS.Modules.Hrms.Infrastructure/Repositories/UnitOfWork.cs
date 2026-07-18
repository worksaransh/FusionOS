using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Infrastructure.Persistence;

namespace FusionOS.Modules.Hrms.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly HrmsDbContext _context;

    public UnitOfWork(HrmsDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
