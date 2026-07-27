using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CreateReworkWorkOrder;

/// <summary>
/// Creates a new WorkOrder to rework a quantity of a Completed source work order's
/// scrapped output (Rework, Phase 6, 2026-07-22). The source order's QuantityReworked
/// is incremented (WorkOrder.RecordRework) so the same scrap can't be claimed twice; the
/// new order re-snapshots the same BOM's components and operations, same as a normal
/// CreateWorkOrderCommand, and is linked back via SourceWorkOrderId.
/// </summary>
public sealed record CreateReworkWorkOrderCommand(Guid CompanyId, Guid SourceWorkOrderId, decimal QuantityToRework)
    : ICommand<WorkOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-order.rework" };
    public string EntityType => nameof(Domain.WorkOrders.WorkOrder);
    public Guid EntityId => SourceWorkOrderId;
    public string Action => "ReworkCreated";
}
