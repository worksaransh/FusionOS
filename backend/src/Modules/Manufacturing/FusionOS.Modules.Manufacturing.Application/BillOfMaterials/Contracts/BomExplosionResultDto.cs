namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

/// <summary>One flattened, summed-by-product leaf requirement from a multi-level BOM explosion (Phase 6, 2026-07-23) — a ComponentProductId that has no BillOfMaterials of its own (i.e. Inventory needs to actually supply/stock it), with every occurrence across every level/branch of the exploded tree summed into one quantity.</summary>
public sealed record BomExplosionLineDto(Guid ProductId, decimal TotalQuantityRequired);

/// <summary>
/// Result of exploding a <see cref="Domain.BillOfMaterials.BillOfMaterials"/> down to its
/// leaf (non-manufactured) component requirements — Multi-Level BOM, Phase 6, 2026-07-23.
/// <see cref="LeafRequirements"/> is sorted by <see cref="BomExplosionLineDto.TotalQuantityRequired"/>
/// descending (the components needed in the largest quantity are the ones most worth a
/// planner's attention first) — not by ProductId, which would carry no useful ordering
/// signal for this result.
/// </summary>
public sealed record BomExplosionResultDto(
    Guid RootBillOfMaterialsId,
    decimal RequestedQuantity,
    IReadOnlyList<BomExplosionLineDto> LeafRequirements,
    IReadOnlyList<string> Warnings);
