using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.RemoveRoutingOperation;

public sealed record RemoveRoutingOperationCommand(Guid CompanyId, Guid BillOfMaterialsId, Guid OperationId)
    : ICommand<BillOfMaterialsDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.routing-operation.remove" };
    public string EntityType => nameof(Domain.BillOfMaterials.BillOfMaterials);
    public Guid EntityId => BillOfMaterialsId;
    public string Action => "RoutingOperationRemoved";
}
