using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Commands.PackPickList;

public sealed record PackPickListCommand(Guid CompanyId, Guid Id)
    : ICommand<PickListDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.pick-list.pack" };
    public string EntityType => nameof(Domain.PickLists.PickList);
    public Guid EntityId => Id;
    public string Action => "Packed";
}
