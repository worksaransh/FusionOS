using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.RecordSparePartUsage;

/// <summary>
/// WarehouseId is required — an opaque cross-module reference into the Warehouse
/// module, same "caller-supplied, never validated" convention as
/// Inventory's own AdjustStockCommand.WarehouseId. It is what lets Inventory's
/// SparePartsConsumedForMaintenanceConsumer post a real Stock Ledger movement:
/// Asset deliberately carries no WarehouseId of its own (see Asset's doc comment), so
/// the warehouse a part is drawn from can only be supplied at the point of usage.
/// </summary>
public sealed record RecordSparePartUsageCommand(Guid CompanyId, Guid MaintenanceRequestId, Guid ProductId, Guid WarehouseId, decimal QuantityUsed, decimal UnitCost)
    : ICommand<MaintenanceRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.spare-part-usage.record" };
    public string EntityType => nameof(Domain.MaintenanceRequests.MaintenanceRequest);
    public Guid EntityId => MaintenanceRequestId;
    public string Action => "SparePartUsageRecorded";
}
