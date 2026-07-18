using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using FusionOS.Modules.Manufacturing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Manufacturing.Infrastructure.Repositories;

public sealed class WorkOrderRepository : IWorkOrderRepository
{
    private readonly ManufacturingDbContext _context;

    public WorkOrderRepository(ManufacturingDbContext context) => _context = context;

    // .Include(x => x.Components) is required because the Components collection is backed by a private field.
    public Task<WorkOrder?> GetByIdAsync(Guid companyId, Guid workOrderId, CancellationToken cancellationToken = default) =>
        _context.WorkOrders.Include(w => w.Components).FirstOrDefaultAsync(w => w.CompanyId == companyId && w.Id == workOrderId, cancellationToken);

    public async Task AddAsync(WorkOrder workOrder, CancellationToken cancellationToken = default) =>
        await _context.WorkOrders.AddAsync(workOrder, cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.WorkOrders
            .Include(w => w.Components)
            .Where(w => w.CompanyId == companyId)
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.WorkOrders.CountAsync(w => w.CompanyId == companyId, cancellationToken);
}
