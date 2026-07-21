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
}
