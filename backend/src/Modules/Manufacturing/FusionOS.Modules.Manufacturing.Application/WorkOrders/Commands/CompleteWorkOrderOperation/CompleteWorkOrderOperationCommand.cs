using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CompleteWorkOrderOperation;

/// <summary>Completes one of a Released work order's snapshotted routing operations (Phase 6, 2026-07-22) — see WorkOrder.CompleteOperation.</summary>
public sealed record CompleteWorkOrderOperationCommand(Guid CompanyId, Guid WorkOrderId, Guid WorkOrderOperationId)
    : ICommand<WorkOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-order.complete-operation" };
    public string EntityType => nameof(Domain.WorkOrders.WorkOrder);
    public Guid EntityId => WorkOrderId;
    public string Action => "OperationCompleted";
}
