using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Infrastructure.Persistence;

namespace FusionOS.Modules.Procurement.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ProcurementDbContext _context;

    public UnitOfWork(ProcurementDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
