using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Commands.RecordPick;

public sealed record RecordPickCommand(Guid CompanyId, Guid Id, Guid LineId, decimal QuantityPicked)
    : ICommand<PickListDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.pick-list.record" };
    public string EntityType => nameof(Domain.PickLists.PickList);
    public Guid EntityId => Id;
    public string Action => "Recorded";
}
