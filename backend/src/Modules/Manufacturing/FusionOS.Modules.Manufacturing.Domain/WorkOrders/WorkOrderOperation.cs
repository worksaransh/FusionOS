namespace FusionOS.Modules.Manufacturing.Domain.WorkOrders;

/// <summary>
/// Operation-level tracking for a <see cref="WorkOrder"/> (Phase 6, 2026-07-22) — the
/// gap the Phase 3 audit flagged: a WorkOrder previously only snapshotted BOM
/// components, never the BOM's RoutingOperations, so there was no way to track "which
/// operation is this order currently at" or schedule a specific operation onto a
/// machine. Snapshotted from the parent BillOfMaterials' Operations at WorkOrder
/// creation, same "capture at the point of the transaction" discipline as
/// WorkOrderComponent — a later edit to the BOM's routing never retroactively changes
/// an in-flight or completed work order's operations.
///
/// RoutingOperationId is the source BillOfMaterials.RoutingOperation this was snapshotted
/// from (same-module reference, not existence-validated after the fact). MachineId is an
/// opaque same-module reference to Machine, set only via Schedule (Machine Scheduling,
/// Phase 6) — conflict-checking against the machine's other schedules is the
/// ScheduleWorkOrderOperationCommandHandler's job (it must query across work orders),
/// not this entity's, since one aggregate cannot see another's state.
/// </summary>
public sealed class WorkOrderOperation
{
    public Guid Id { get; private set; }
    public Guid RoutingOperationId { get; private set; }
    public int SequenceNumber { get; private set; }
    public string OperationName { get; private set; } = default!;
    public string WorkCenter { get; private set; } = default!;
    public decimal StandardMinutes { get; private set; }
    public WorkOrderOperationStatus Status { get; private set; } = WorkOrderOperationStatus.Pending;

    public Guid? MachineId { get; private set; }
    public DateTimeOffset? ScheduledStart { get; private set; }
    public DateTimeOffset? ScheduledEnd { get; private set; }
    public DateTimeOffset? ActualStart { get; private set; }
    public DateTimeOffset? ActualEnd { get; private set; }

    private WorkOrderOperation() { }

    internal static WorkOrderOperation Create(Guid routingOperationId, int sequenceNumber, string operationName, string workCenter, decimal standardMinutes)
    {
        if (routingOperationId == Guid.Empty)
            throw new ArgumentException("Routing operation id is required.", nameof(routingOperationId));
        if (string.IsNullOrWhiteSpace(operationName))
            throw new ArgumentException("Operation name is required.", nameof(operationName));
        if (string.IsNullOrWhiteSpace(workCenter))
            throw new ArgumentException("Work center is required.", nameof(workCenter));

        return new WorkOrderOperation
        {
            Id = Guid.NewGuid(),
            RoutingOperationId = routingOperationId,
            SequenceNumber = sequenceNumber,
            OperationName = operationName,
            WorkCenter = workCenter,
            StandardMinutes = standardMinutes,
        };
    }

    /// <summary>Pending → InProgress, stamping ActualStart. Not gated on sequence order — the shop floor, not this entity, decides which operation to start next.</summary>
    internal void Start(DateTimeOffset now)
    {
        if (Status != WorkOrderOperationStatus.Pending)
            throw new InvalidOperationException($"Only a Pending operation can be started (current status: {Status}).");

        Status = WorkOrderOperationStatus.InProgress;
        ActualStart = now;
    }

    /// <summary>InProgress → Completed, stamping ActualEnd.</summary>
    internal void Complete(DateTimeOffset now)
    {
        if (Status != WorkOrderOperationStatus.InProgress)
            throw new InvalidOperationException($"Only an InProgress operation can be completed (current status: {Status}).");

        Status = WorkOrderOperationStatus.Completed;
        ActualEnd = now;
    }

    /// <summary>Assigns a machine and a scheduled time window (Machine Scheduling, Phase 6). Only permitted while Pending — a started or completed operation is no longer something to schedule.</summary>
    internal void Schedule(Guid machineId, DateTimeOffset scheduledStart, DateTimeOffset scheduledEnd)
    {
        if (Status != WorkOrderOperationStatus.Pending)
            throw new InvalidOperationException($"Only a Pending operation can be scheduled (current status: {Status}).");
        if (machineId == Guid.Empty)
            throw new ArgumentException("Machine id is required.", nameof(machineId));
        if (scheduledEnd <= scheduledStart)
            throw new ArgumentException("Scheduled end must be after scheduled start.", nameof(scheduledEnd));

        MachineId = machineId;
        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledEnd;
    }
}
