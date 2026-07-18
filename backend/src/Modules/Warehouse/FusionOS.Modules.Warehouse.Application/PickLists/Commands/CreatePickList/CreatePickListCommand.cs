using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using FusionOS.Modules.Warehouse.Domain.PickLists;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Commands.CreatePickList;

/// <summary>
/// SalesOrderId is an opaque cross-module reference — not validated for existence here (see
/// PickList.cs's doc comment for why). BinId per line, when supplied, IS validated by the handler
/// since Bin lives in this same module.
/// </summary>
public sealed record CreatePickListCommand(Guid CompanyId, Guid WarehouseId, Guid SalesOrderId, IReadOnlyList<PickListLineInput> Lines)
    : ICommand<PickListDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.pick-list.create" };
    public string EntityType => nameof(Domain.PickLists.PickList);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
