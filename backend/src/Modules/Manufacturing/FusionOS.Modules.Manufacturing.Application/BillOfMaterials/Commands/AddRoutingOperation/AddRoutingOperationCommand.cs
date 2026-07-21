using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.AddRoutingOperation;

public sealed record AddRoutingOperationCommand(Guid CompanyId, Guid BillOfMaterialsId, string OperationName, string WorkCenter, decimal StandardMinutes)
    : ICommand<BillOfMaterialsDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.routing-operation.create" };
    public string EntityType => nameof(Domain.BillOfMaterials.BillOfMaterials);
    public Guid EntityId => BillOfMaterialsId;
    public string Action => "RoutingOperationAdded";
}
