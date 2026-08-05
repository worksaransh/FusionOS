using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.UpdateBillOfMaterials;

/// <summary>
/// Header-level-only update (Name, ProductId) — deliberately distinct from
/// CreateNewBomVersionCommand: it does not touch Lines/Operations and does not bump
/// Version. Code is excluded — it's the immutable business key, same convention as
/// UpdateBranchCommand/UpdateCostCenterCommand.
/// </summary>
public sealed record UpdateBillOfMaterialsCommand(Guid CompanyId, Guid BillOfMaterialsId, string Name, Guid ProductId)
    : ICommand<BillOfMaterialsDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.bill-of-materials.update" };
    public string EntityType => nameof(Domain.BillOfMaterials.BillOfMaterials);
    public Guid EntityId => BillOfMaterialsId;
    public string Action => "Updated";
}
