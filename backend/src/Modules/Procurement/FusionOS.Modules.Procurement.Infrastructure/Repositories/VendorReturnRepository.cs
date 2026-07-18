using FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;
using FusionOS.Modules.Procurement.Domain.VendorReturns;
using FusionOS.Modules.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Procurement.Infrastructure.Repositories;

public sealed class VendorReturnRepository : IVendorReturnRepository
{
    private readonly ProcurementDbContext _context;

    public VendorReturnRepository(ProcurementDbContext context) => _context = context;

    public Task<VendorReturn?> GetByIdAsync(Guid companyId, Guid vendorReturnId, CancellationToken cancellationToken = default) =>
        _context.VendorReturns.FirstOrDefaultAsync(v => v.CompanyId == companyId && v.Id == vendorReturnId, cancellationToken);

    public async Task AddAsync(VendorReturn vendorReturn, CancellationToken cancellationToken = default) =>
        await _context.VendorReturns.AddAsync(vendorReturn, cancellationToken);

    public async Task<IReadOnlyList<VendorReturn>> ListAsync(Guid companyId, Guid? purchaseOrderId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, purchaseOrderId)
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? purchaseOrderId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, purchaseOrderId).CountAsync(cancellationToken);

    public Task<decimal> SumReturnedQuantityAsync(Guid companyId, Guid purchaseOrderId, Guid productId, CancellationToken cancellationToken = default) =>
        _context.VendorReturns
            .Where(v => v.CompanyId == companyId && v.PurchaseOrderId == purchaseOrderId && v.ProductId == productId && v.Status != VendorReturnStatus.Cancelled)
            .SumAsync(v => v.Quantity, cancellationToken);

    private IQueryable<VendorReturn> Filtered(Guid companyId, Guid? purchaseOrderId)
    {
        var query = _context.VendorReturns.Where(v => v.CompanyId == companyId);
        if (purchaseOrderId.HasValue)
            query = query.Where(v => v.PurchaseOrderId == purchaseOrderId.Value);
        return query;
    }
}
