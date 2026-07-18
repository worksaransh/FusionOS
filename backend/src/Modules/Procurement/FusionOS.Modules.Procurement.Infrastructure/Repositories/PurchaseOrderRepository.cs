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
        // TotalAmount is EF-Ignore()'d (computed in-memory from _lines.Sum(...)),
        // so it can't be summed via SQL translation — materialize matching orders
        // first, then group/sum in memory. Same fix already documented on
        // InvoiceRepository.GetIssuedInvoiceTotalsBySalesPersonAsync.
        var orders = await _context.PurchaseOrders
            .Include(x => x.Lines)
            .Where(x => x.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        return orders
            .GroupBy(o => o.SupplierId)
            .Select(g => (
                SupplierId: g.Key,
                OrderCount: g.Count(),
                TotalOrderValue: g.Sum(o => o.TotalAmount),
                FullyReceivedCount: g.Count(o => o.Status == PurchaseOrderStatus.FullyReceived)))
            .ToList();
    }

    public async Task<IReadOnlyList<(Guid PurchaseOrderId, Guid SupplierId, DateTimeOffset OrderDate, decimal UnitPrice, decimal Quantity)>> GetPriceHistoryAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default)
    {
        // Same "materialize then project in memory" fix as GetSupplierOrderStatsAsync above —
        // Lines is a navigation collection, not something SQL can flatten alongside OrderDate/Id in one translated query.
        var orders = await _context.PurchaseOrders
            .Include(x => x.Lines)
            .Where(x => x.CompanyId == companyId && x.Lines.Any(l => l.ProductId == productId))
            .OrderBy(x => x.OrderDate)
            .ToListAsync(cancellationToken);

        return orders
            .SelectMany(o => o.Lines
                .Where(l => l.ProductId == productId)
                .Select(l => (PurchaseOrderId: o.Id, SupplierId: o.SupplierId, OrderDate: o.OrderDate, UnitPrice: l.UnitPrice, Quantity: l.Quantity)))
            .ToList();
    }
}
