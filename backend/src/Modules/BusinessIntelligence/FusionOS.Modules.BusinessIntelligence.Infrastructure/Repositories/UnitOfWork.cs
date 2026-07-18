using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using FusionOS.Modules.BusinessIntelligence.Infrastructure.Persistence;

namespace FusionOS.Modules.BusinessIntelligence.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BusinessIntelligenceDbContext _context;

    public UnitOfWork(BusinessIntelligenceDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
