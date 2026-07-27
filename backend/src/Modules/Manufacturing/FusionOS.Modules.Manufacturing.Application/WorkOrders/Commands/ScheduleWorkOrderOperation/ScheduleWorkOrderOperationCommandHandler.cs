using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.Machines.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.ScheduleWorkOrderOperation;

/// <summary>
/// Machine Scheduling (Phase 6, 2026-07-22). WorkOrder.ScheduleOperation's own doc
/// comment is explicit that cross-work-order conflict checking is this handler's job,
/// not the aggregate's — one WorkOrder aggregate cannot see another's scheduled
/// operations, so this handler queries every Released work order's operations already
/// scheduled onto the target machine (IWorkOrderRepository.ListScheduledOperationsByMachineAsync,
/// filtered server-side by machine so this doesn't load every Released work order's full
/// object graph company-wide) and checks for a time-window overlap before delegating to
/// the aggregate.
///
/// Rescheduling: WorkOrderOperation.Schedule only accepts Pending operations and
/// scheduling doesn't itself change status, so an operation that already has a machine
/// assigned can still (per the aggregate) be "re-scheduled" while still Pending. The
/// conflict check below excludes the operation being rescheduled (by
/// WorkOrderOperationId) from its own candidate set, so moving the same operation to a
/// slightly different window is not falsely rejected as conflicting with its own prior
/// (soon-to-be-replaced) schedule.
/// </summary>
public sealed class ScheduleWorkOrderOperationCommandHandler : IRequestHandler<ScheduleWorkOrderOperationCommand, WorkOrderDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IMachineRepository _machineRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleWorkOrderOperationCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IMachineRepository machineRepository,
        IUnitOfWork unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _machineRepository = machineRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkOrderDto> Handle(ScheduleWorkOrderOperationCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetByIdAsync(request.CompanyId, request.WorkOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Work order '{request.WorkOrderId}' was not found.");

        var machine = await _machineRepository.GetByIdAsync(request.CompanyId, request.MachineId, cancellationToken)
            ?? throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.MachineId), $"Machine '{request.MachineId}' does not exist for this company."),
            });

        // Machines are only scheduled against work that is actually on the shop floor —
        // Draft work orders aren't released yet, so their operations can't conflict.
        // Filtered server-side to this one machine's already-scheduled operations rather
        // than loading every Released work order's full Components/Operations graph
        // company-wide just to discard everything not on this machine (was an unbounded
        // load on every schedule call).
        var candidates = await _workOrderRepository.ListScheduledOperationsByMachineAsync(request.CompanyId, machine.Id, cancellationToken);

        var conflict = candidates.FirstOrDefault(x =>
            x.OperationId != request.WorkOrderOperationId
            && x.ScheduledStart < request.ScheduledEnd
            && request.ScheduledStart < x.ScheduledEnd);

        if (conflict is not null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.ScheduledStart),
                    $"Machine '{machine.Code}' is already scheduled on work order '{conflict.WorkOrderId}', operation '{conflict.OperationName}' " +
                    $"from {conflict.ScheduledStart:O} to {conflict.ScheduledEnd:O}, which overlaps the requested window."),
            });
        }

        workOrder.ScheduleOperation(request.WorkOrderOperationId, machine.Id, request.ScheduledStart, request.ScheduledEnd);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return WorkOrderMapper.ToDto(workOrder);
    }
}
