using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using FusionOS.Modules.Sales.Domain.Dispatches;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class DispatchRepository : IDispatchRepository
{
    private readonly SalesDbContext _context;

    public DispatchRepository(SalesDbContext context) => _context = context;

    public async Task AddAsync(Dispatch dispatch, CancellationToken cancellationToken = default) =>
        await _context.Dispatches.AddAsync(dispatch, cancellationToken);

    public async Task<IReadOnlyList<Dispatch>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Dispatches
            .Include(d => d.Lines)
            .Where(d => d.CompanyId == companyId)
            .OrderByDescending(d => d.DispatchDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.Dispatches.CountAsync(d => d.CompanyId == companyId, cancellationToken);

    // Loads matching dispatches with their lines and sums in memory rather than
    // relying on EF translating a SelectMany over DispatchLine (a private-field-backed
    // owned collection) directly into SQL - safer to verify by reading given no
    // compiler is available in this environment (2026-07-14 coverage-audit follow-up).
    public async Task<decimal> GetDispatchedQuantityAsync(Guid companyId, Guid salesOrderId, Guid productId, CancellationToken cancellationToken = default)
    {
        var dispatches = await _context.Dispatches
            .Include(d => d.Lines)
            .Where(d => d.CompanyId == companyId && d.SalesOrderId == salesOrderId)
            .ToListAsync(cancellationToken);

        return dispatches
            .SelectMany(d => d.Lines)
            .Where(l => l.ProductId == productId)
            .Sum(l => l.QuantityDispatched);
    }
}
