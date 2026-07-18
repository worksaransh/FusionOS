using FusionOS.Modules.Sales.Application.PriceLists.Contracts;
using FusionOS.Modules.Sales.Domain.PriceLists;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class PriceListRepository : IPriceListRepository
{
    private readonly SalesDbContext _context;

    public PriceListRepository(SalesDbContext context) => _context = context;

    public Task<PriceList?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.PriceLists
            .Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid priceListId, CancellationToken cancellationToken = default) =>
        _context.PriceLists.AnyAsync(x => x.CompanyId == companyId && x.Id == priceListId, cancellationToken);

    public async Task AddAsync(PriceList priceList, CancellationToken cancellationToken = default) =>
        await _context.PriceLists.AddAsync(priceList, cancellationToken);

    public async Task<IReadOnlyList<PriceList>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.PriceLists
            .Include(x => x.Entries)
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.PriceLists.CountAsync(x => x.CompanyId == companyId, cancellationToken);
}
