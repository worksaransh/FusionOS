using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FusionOS.Modules.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Procurement.Infrastructure.Repositories;

public sealed class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly ProcurementDbContext _context;

    public PurchaseOrderRepository(ProcurementDbContext context) => _context = context;

    public Task<PurchaseOrder?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.PurchaseOrders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken);

    public async Task AddAsync(PurchaseOrder order, CancellationToken cancellationToken = default) =>
        await _context.PurchaseOrders.AddAsync(order, cancellationToken);

    public async Task<IReadOnlyList<PurchaseOrder>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.PurchaseOrders
            .Include(x => x.Lines)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.PurchaseOrders.CountAsync(x => x.CompanyId == companyId, cancellationToken);

    public async Task<IReadOnlyList<(PurchaseOrderStatus Status, int Count)>> CountByStatusAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var grouped = await _context.PurchaseOrders
            .Where(x => x.CompanyId == companyId)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return grouped.Select(g => (g.Status, g.Count)).ToList();
    }

    public async Task<IReadOnlyList<(Guid SupplierId, int OrderCount, decimal TotalOrderValue, int FullyReceivedCount)>> GetSupplierOrderStatsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        // TotalAmount itself is EF-Ignore()'d (it's a computed in-memory property,
        // _lines.Sum(l => l.LineTotal)), but LineTotal — the persisted column it's
        // built from — is a real mapped property. So instead of materializing every
        // purchase order (with every line) the company has ever created and grouping
        // in memory, this runs two SQL-translated grouped aggregations and merges the
        // (small, one-row-per-supplier) results in memory:
        //  - order-level counts straight off PurchaseOrders, same GroupBy/Count shape
        //    as CountByStatusAsync above;
        //  - TotalOrderValue via the exact from-from-groupby-line flattening template
        //    JournalEntryRepository uses for its line aggregations. Lines is a plain
        //    FK-mapped HasMany/WithOne collection (see PurchaseOrderConfiguration),
        //    not an EF owned type, so it flattens/groups in SQL the same way.
        var counts = await _context.PurchaseOrders
            .Where(x => x.CompanyId == companyId)
            .GroupBy(x => x.SupplierId)
            .Select(g => new
            {
                SupplierId = g.Key,
                OrderCount = g.Count(),
                FullyReceivedCount = g.Count(o => o.Status == PurchaseOrderStatus.FullyReceived),
            })
            .ToListAsync(cancellationToken);

        var totals = await (
            from po in _context.PurchaseOrders
            where po.CompanyId == companyId
            from line in po.Lines
            group line by po.SupplierId into g
            select new { SupplierId = g.Key, TotalOrderValue = g.Sum(l => l.LineTotal) })
            .ToListAsync(cancellationToken);

        var totalsBySupplier = totals.ToDictionary(t => t.SupplierId, t => t.TotalOrderValue);

        return counts
            .Select(c => (
                c.SupplierId,
                c.OrderCount,
                totalsBySupplier.TryGetValue(c.SupplierId, out var total) ? total : 0m,
                c.FullyReceivedCount))
            .ToList();
    }

    public async Task<IReadOnlyList<(Guid PurchaseOrderId, Guid SupplierId, DateTimeOffset OrderDate, decimal UnitPrice, decimal Quantity)>> GetPriceHistoryAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default)
    {
        // Pushes the ProductId filter into the query itself (same flattened
        // from-from-select shape as JournalEntryRepository's line aggregations) so
        // only the lines that actually match productId are ever transferred from
        // the database, instead of loading every purchase order (with all of its
        // lines) that ever touched the product and filtering client-side.
        var rows = await (
            from po in _context.PurchaseOrders
            where po.CompanyId == companyId
            from line in po.Lines
            where line.ProductId == productId
            orderby po.OrderDate
            select new
            {
                PurchaseOrderId = po.Id,
                SupplierId = po.SupplierId,
                OrderDate = po.OrderDate,
                UnitPrice = line.UnitPrice,
                Quantity = line.Quantity,
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => (r.PurchaseOrderId, r.SupplierId, r.OrderDate, r.UnitPrice, r.Quantity)).ToList();
    }
}
