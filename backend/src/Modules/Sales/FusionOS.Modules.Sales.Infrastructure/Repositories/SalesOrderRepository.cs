using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class SalesOrderRepository : ISalesOrderRepository
{
    private readonly SalesDbContext _context;

    public SalesOrderRepository(SalesDbContext context) => _context = context;

    public Task<SalesOrder?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.SalesOrders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken);

    public async Task AddAsync(SalesOrder order, CancellationToken cancellationToken = default) =>
        await _context.SalesOrders.AddAsync(order, cancellationToken);

    public async Task<IReadOnlyList<SalesOrder>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.SalesOrders
            .Include(x => x.Lines)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.SalesOrders.CountAsync(x => x.CompanyId == companyId, cancellationToken);
}
