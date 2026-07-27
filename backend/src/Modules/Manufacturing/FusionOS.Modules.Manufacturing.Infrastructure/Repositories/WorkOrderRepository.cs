using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using FusionOS.Modules.Manufacturing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Manufacturing.Infrastructure.Repositories;

public sealed class WorkOrderRepository : IWorkOrderRepository
{
    private readonly ManufacturingDbContext _context;

    public WorkOrderRepository(ManufacturingDbContext context) => _context = context;

    // .Include(x => x.Components)/.Include(x => x.Operations) are required because both collections are backed by private fields.
    public Task<WorkOrder?> GetByIdAsync(Guid companyId, Guid workOrderId, CancellationToken cancellationToken = default) =>
        _context.WorkOrders.Include(w => w.Components).Include(w => w.Operations).FirstOrDefaultAsync(w => w.CompanyId == companyId && w.Id == workOrderId, cancellationToken);

    public async Task AddAsync(WorkOrder workOrder, CancellationToken cancellationToken = default) =>
        await _context.WorkOrders.AddAsync(workOrder, cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.WorkOrders
            .Include(w => w.Components)
            .Include(w => w.Operations)
            .Where(w => w.CompanyId == companyId)
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.WorkOrders.CountAsync(w => w.CompanyId == companyId, cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> ListByStatusAsync(Guid companyId, WorkOrderStatus status, CancellationToken cancellationToken = default) =>
        await _context.WorkOrders
            .Include(w => w.Components)
            .Include(w => w.Operations)
            .Where(w => w.CompanyId == companyId && w.Status == status)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ScheduledOperationConflictCandidate>> ListScheduledOperationsByMachineAsync(Guid companyId, Guid machineId, CancellationToken cancellationToken = default) =>
        await _context.WorkOrders
            .Where(w => w.CompanyId == companyId && w.Status == WorkOrderStatus.Released)
            .SelectMany(w => w.Operations
                .Where(op => op.MachineId == machineId && op.ScheduledStart != null && op.ScheduledEnd != null)
                .Select(op => new ScheduledOperationConflictCandidate(op.Id, w.Id, op.OperationName, op.ScheduledStart!.Value, op.ScheduledEnd!.Value)))
            .ToListAsync(cancellationToken);
}
