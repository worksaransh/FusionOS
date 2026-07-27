using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.ExplodeBillOfMaterials;

/// <summary>
/// Multi-Level BOM explosion (Phase 6, 2026-07-23): given one BillOfMaterials and a
/// quantity of its parent product to build, walks every component line down through
/// any of ITS OWN component's separate BillOfMaterials (BillOfMaterials is deliberately
/// flat — see its doc comment) and returns the flattened, summed total of every leaf
/// (non-manufactured) component required. See <see cref="Contracts.BomExplosionEngine"/>
/// for the recursion/cycle-guard logic itself.
///
/// Gated by the same manufacturing.bill-of-materials.read permission every other
/// read-only BOM query uses — this only reads BillOfMaterials/BomLine master data.
/// </summary>
public sealed record ExplodeBillOfMaterialsQuery(Guid CompanyId, Guid BillOfMaterialsId, decimal Quantity)
    : IQuery<BomExplosionResultDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.bill-of-materials.read" };
}
