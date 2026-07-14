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
}
