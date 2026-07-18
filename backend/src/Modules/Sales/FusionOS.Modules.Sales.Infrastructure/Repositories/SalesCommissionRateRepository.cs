using FusionOS.Modules.Sales.Application.Commissions.Contracts;
using FusionOS.Modules.Sales.Domain.Commissions;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class SalesCommissionRateRepository : ISalesCommissionRateRepository
{
    private readonly SalesDbContext _context;

    public SalesCommissionRateRepository(SalesDbContext context) => _context = context;

    public Task<SalesCommissionRate?> GetByUserIdAsync(Guid companyId, Guid userId, CancellationToken cancellationToken = default) =>
        _context.SalesCommissionRates
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.UserId == userId, cancellationToken);

    public async Task AddAsync(SalesCommissionRate rate, CancellationToken cancellationToken = default) =>
        await _context.SalesCommissionRates.AddAsync(rate, cancellationToken);

    public async Task<IReadOnlyList<SalesCommissionRate>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.SalesCommissionRates
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.UserId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.SalesCommissionRates.CountAsync(x => x.CompanyId == companyId, cancellationToken);
}
