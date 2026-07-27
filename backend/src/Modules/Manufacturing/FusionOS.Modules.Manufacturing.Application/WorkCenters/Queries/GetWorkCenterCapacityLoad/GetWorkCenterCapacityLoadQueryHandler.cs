using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Queries.GetWorkCenterCapacityLoad;

/// <summary>
/// Capacity Planning (Phase 6, 2026-07-22). Two important limitations, both intentional
/// for this first slice:
///
/// 1. Matching a WorkOrderOperation to this WorkCenter is a best-effort, case-insensitive
///    string match against the operation's free-text WorkCenter field (matched against
///    either this WorkCenter's Code or Name) — NOT a hard foreign key. RoutingOperation
///    and WorkOrderOperation predate the WorkCenter entity and their WorkCenter field
///    stays free text (see WorkCenter's own doc comment), so a typo'd or renamed
///    work-center label on an operation simply won't be counted here.
///
/// 2. Only operations that have already been scheduled (ScheduledStart/ScheduledEnd set
///    via ScheduleWorkOrderOperationCommand) count toward RequiredHours — an unscheduled
///    Pending operation has no known date to test against [PeriodStart, PeriodEnd], so it
///    is silently excluded rather than guessed at.
/// </summary>
public sealed class GetWorkCenterCapacityLoadQueryHandler : IRequestHandler<GetWorkCenterCapacityLoadQuery, WorkCenterCapacityLoadDto>
{
    private readonly IWorkCenterRepository _workCenterRepository;
    private readonly IWorkOrderRepository _workOrderRepository;

    public GetWorkCenterCapacityLoadQueryHandler(IWorkCenterRepository workCenterRepository, IWorkOrderRepository workOrderRepository)
    {
        _workCenterRepository = workCenterRepository;
        _workOrderRepository = workOrderRepository;
    }

    public async Task<WorkCenterCapacityLoadDto> Handle(GetWorkCenterCapacityLoadQuery request, CancellationToken cancellationToken)
    {
        var workCenter = await _workCenterRepository.GetByIdAsync(request.CompanyId, request.WorkCenterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Work center '{request.WorkCenterId}' was not found.");

        var days = (request.PeriodEnd.Date - request.PeriodStart.Date).Days + 1;
        var availableHours = workCenter.CapacityHoursPerDay * Math.Max(days, 0);

        var releasedWorkOrders = await _workOrderRepository.ListByStatusAsync(request.CompanyId, WorkOrderStatus.Released, cancellationToken);

        var requiredMinutes = releasedWorkOrders
            .SelectMany(wo => wo.Operations)
            .Where(op => op.ScheduledStart.HasValue
                && op.ScheduledStart.Value >= request.PeriodStart
                && op.ScheduledStart.Value <= request.PeriodEnd
                && (string.Equals(op.WorkCenter, workCenter.Code, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(op.WorkCenter, workCenter.Name, StringComparison.OrdinalIgnoreCase)))
            .Sum(op => op.StandardMinutes);

        var requiredHours = requiredMinutes / 60m;
        var utilizationPercentage = availableHours > 0 ? Math.Round(requiredHours / availableHours * 100m, 2) : 0m;

        return new WorkCenterCapacityLoadDto(workCenter.Id, availableHours, requiredHours, utilizationPercentage);
    }
}
