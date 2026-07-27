namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

public interface IWorkOrderRepository
{
    Task<Domain.WorkOrders.WorkOrder?> GetByIdAsync(Guid companyId, Guid workOrderId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.WorkOrders.WorkOrder workOrder, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.WorkOrders.WorkOrder>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Every work order in a given status, unpaged (WIP support, Phase 6) — backs GetWorkInProgressSummaryReportQuery, which needs every Released order's component issuance in one shot rather than one page at a time.</summary>
    Task<IReadOnlyList<Domain.WorkOrders.WorkOrder>> ListByStatusAsync(Guid companyId, Domain.WorkOrders.WorkOrderStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every already-scheduled operation (MachineId set) on a Released work order for the given
    /// machine — backs ScheduleWorkOrderOperationCommandHandler's cross-work-order conflict check.
    /// Filters by machine (and Released status) server-side rather than loading every Released
    /// work order's full Components/Operations graph company-wide just to discard everything not
    /// on this one machine (was an unbounded, ever-growing load on every schedule call).
    /// </summary>
    Task<IReadOnlyList<ScheduledOperationConflictCandidate>> ListScheduledOperationsByMachineAsync(Guid companyId, Guid machineId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Projection backing ListScheduledOperationsByMachineAsync's conflict check — just enough to
/// identify and report an overlapping schedule without materializing the owning
/// WorkOrder/WorkOrderOperation entities. OperationId lets the handler exclude a candidate's own
/// prior scheduled window when rescheduling that same operation, so it isn't falsely reported as
/// conflicting with itself.
/// </summary>
public sealed record ScheduledOperationConflictCandidate(Guid OperationId, Guid WorkOrderId, string OperationName, DateTimeOffset ScheduledStart, DateTimeOffset ScheduledEnd);
