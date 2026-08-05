using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using FusionOS.Modules.Sales.Domain.Dispatches;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class DispatchRepository : IDispatchRepository
{
    private readonly SalesDbContext _context;

    public DispatchRepository(SalesDbContext context) => _context = context;

    public Task<Dispatch?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.Dispatches.Include(d => d.Lines).FirstOrDefaultAsync(d => d.CompanyId == companyId && d.Id == id, cancellationToken);

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

    // DispatchLine is a plain FK-mapped entity (DispatchConfiguration: HasMany(...).WithOne()
    // .HasForeignKey("DispatchId")), not an EF owned type, so a SelectMany over the Lines
    // navigation translates to a single SQL join+SUM - same shape as
    // JournalEntryRepository.SumPostedAmountByAccountAsync (Finance). Fixed 2026-07-21:
    // the previous version loaded every dispatch + all lines for the sales order into
    // memory and summed client-side.
    public Task<decimal> GetDispatchedQuantityAsync(Guid companyId, Guid salesOrderId, Guid productId, CancellationToken cancellationToken = default)
    {
        var lines =
            from dispatch in _context.Dispatches
            where dispatch.CompanyId == companyId && dispatch.SalesOrderId == salesOrderId
            from line in dispatch.Lines
            where line.ProductId == productId
            select line;

        return lines.SumAsync(l => l.QuantityDispatched, cancellationToken);
    }

    // Batch counterpart of GetDispatchedQuantityAsync above (Phase 10 audit fix,
    // 2026-07-27): groups by ProductId in a single SQL join+GROUP BY+SUM instead
    // of one round trip per distinct product in the caller's GroupBy loop.
    public async Task<IReadOnlyDictionary<Guid, decimal>> GetDispatchedQuantitiesAsync(Guid companyId, Guid salesOrderId, IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default)
    {
        var grouped = await (
            from dispatch in _context.Dispatches
            where dispatch.CompanyId == companyId && dispatch.SalesOrderId == salesOrderId
            from line in dispatch.Lines
            where productIds.Contains(line.ProductId)
            group line by line.ProductId into g
            select new { ProductId = g.Key, Quantity = g.Sum(l => l.QuantityDispatched) })
            .ToListAsync(cancellationToken);

        return grouped.ToDictionary(g => g.ProductId, g => g.Quantity);
    }

    // Dispatch has no CustomerId of its own (only SalesOrderId - see Dispatch's
    // own doc comment), so Customer Portal scoping joins through SalesOrder
    // here rather than filtering Dispatch directly - same no-cross-module-FK,
    // same-DbContext join pattern GetDispatchedQuantityAsync above already uses.
    public async Task<IReadOnlyList<Dispatch>> ListByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query =
            from dispatch in _context.Dispatches
            join order in _context.SalesOrders on dispatch.SalesOrderId equals order.Id
            where order.CustomerId == customerId
            orderby dispatch.DispatchDate descending
            select dispatch;

        return await query
            .Include(d => d.Lines)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var query =
            from dispatch in _context.Dispatches
            join order in _context.SalesOrders on dispatch.SalesOrderId equals order.Id
            where order.CustomerId == customerId
            select dispatch;

        return query.CountAsync(cancellationToken);
    }
}
