using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Commands.AssignPickList;

public sealed record AssignPickListCommand(Guid CompanyId, Guid Id, Guid AssignedToUserId)
    : ICommand<PickListDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.pick-list.assign" };
    public string EntityType => nameof(Domain.PickLists.PickList);
    public Guid EntityId => Id;
    public string Action => "Assigned";
}
