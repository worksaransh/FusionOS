using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.ReorderRoutingOperations;

public sealed record ReorderRoutingOperationsCommand(Guid CompanyId, Guid BillOfMaterialsId, IReadOnlyList<Guid> OrderedOperationIds)
    : ICommand<BillOfMaterialsDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.routing-operation.reorder" };
    public string EntityType => nameof(Domain.BillOfMaterials.BillOfMaterials);
    public Guid EntityId => BillOfMaterialsId;
    public string Action => "RoutingOperationsReordered";
}
