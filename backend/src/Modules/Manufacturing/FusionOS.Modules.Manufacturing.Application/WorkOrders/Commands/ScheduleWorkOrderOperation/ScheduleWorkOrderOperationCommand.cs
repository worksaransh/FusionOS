using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.ScheduleWorkOrderOperation;

public sealed record ScheduleWorkOrderOperationCommand(
    Guid CompanyId,
    Guid WorkOrderId,
    Guid WorkOrderOperationId,
    Guid MachineId,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd)
    : ICommand<WorkOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-order.schedule-operation" };
    public string EntityType => nameof(Domain.WorkOrders.WorkOrder);
    public Guid EntityId => WorkOrderId;
    public string Action => "ScheduledOperation";
}
