using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.SetPrimaryBillOfMaterials;

/// <summary>
/// Alternative BOM (Phase 6, 2026-07-22). Promotes <see cref="BillOfMaterialsId"/> to be the
/// primary BOM for its product, clearing IsPrimary on every other active BOM sharing that
/// ProductId — see BillOfMaterials.IsPrimary's doc comment for why the aggregate itself
/// can't enforce this alone.
/// </summary>
public sealed record SetPrimaryBillOfMaterialsCommand(Guid CompanyId, Guid BillOfMaterialsId)
    : ICommand<BillOfMaterialsDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.bill-of-materials.set-primary" };
    public string EntityType => nameof(Domain.BillOfMaterials.BillOfMaterials);
    public Guid EntityId => BillOfMaterialsId;
    public string Action => "SetPrimary";
}
