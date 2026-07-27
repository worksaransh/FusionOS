namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

/// <summary>
/// The single recursive multi-level BOM explosion algorithm (Phase 6, 2026-07-23), shared
/// by <c>ExplodeBillOfMaterialsQueryHandler</c> and <c>GenerateMrpRequirementsQueryHandler</c>
/// so the riskiest logic in this slice exists exactly once. BillOfMaterials is deliberately
/// flat (see its own doc comment) — a component that is itself manufactured simply has its
/// own separate primary BillOfMaterials for the same ProductId. Exploding "how much of every
/// raw/purchased component do I ultimately need" therefore means walking that chain of
/// separate BOMs at query time, not reading a self-referencing tree.
/// </summary>
internal static class BomExplosionEngine
{
    /// <summary>
    /// Safety net against pathological (but non-cyclic) BOM chains — e.g. a 30-level-deep
    /// sub-assembly hierarchy someone builds by accident. A real cyclic reference is caught
    /// and reported separately via the recursion-path <see cref="HashSet{T}"/> guard below;
    /// this cap exists purely so a very deep-but-valid chain fails safely (as a truncation
    /// warning) instead of consuming unbounded stack/time.
    /// </summary>
    internal const int MaxRecursionDepth = 20;

    /// <summary>
    /// Explodes <paramref name="rootBom"/> for <paramref name="requestedQuantity"/> units of
    /// its parent product, returning every leaf (non-manufactured) component's total required
    /// quantity summed across every level and branch it appears in, plus any warnings raised
    /// while doing so (circular references, depth-cap truncation).
    /// </summary>
    internal static async Task<(Dictionary<Guid, decimal> LeafTotals, List<string> Warnings)> ExplodeAsync(
        IBillOfMaterialsRepository repository,
        Guid companyId,
        Domain.BillOfMaterials.BillOfMaterials rootBom,
        decimal requestedQuantity,
        CancellationToken cancellationToken)
    {
        var leafTotals = new Dictionary<Guid, decimal>();
        var warnings = new List<string>();

        // Tracks the chain of BillOfMaterials ids on the CURRENT recursion path only (added
        // on entry, removed on exit of each frame) — not every BOM visited anywhere in the
        // whole explosion. The same sub-assembly BOM legitimately appearing in two unrelated
        // branches (e.g. the same bracket used in both the left and right assemblies) is
        // normal and must recurse each time; only an ANCESTOR of the current node reappearing
        // (A contains B, B contains A) is a genuine cycle.
        var pathBomIds = new HashSet<Guid>();

        await ExplodeRecursiveAsync(repository, companyId, rootBom, requestedQuantity, pathBomIds, leafTotals, warnings, depth: 0, cancellationToken);

        return (leafTotals, warnings);
    }

    private static async Task ExplodeRecursiveAsync(
        IBillOfMaterialsRepository repository,
        Guid companyId,
        Domain.BillOfMaterials.BillOfMaterials bom,
        decimal multiplier,
        HashSet<Guid> pathBomIds,
        Dictionary<Guid, decimal> leafTotals,
        List<string> warnings,
        int depth,
        CancellationToken cancellationToken)
    {
        if (depth >= MaxRecursionDepth)
        {
            warnings.Add(
                $"Bill of materials '{bom.Id}' was not exploded further because it sits at depth {depth}, " +
                $"at or beyond the maximum supported explosion depth of {MaxRecursionDepth} levels — its own " +
                "component lines were recorded as leaf requirements instead of being recursed into.");

            foreach (var line in bom.Lines)
                Accumulate(leafTotals, line.ComponentProductId, line.Quantity * multiplier);

            return;
        }

        pathBomIds.Add(bom.Id);

        foreach (var line in bom.Lines)
        {
            var requiredQuantity = line.Quantity * multiplier;

            var childBom = await repository.GetPrimaryByProductIdAsync(companyId, line.ComponentProductId, cancellationToken);
            if (childBom is null)
            {
                // No BOM of its own — this component is a leaf, sourced from Inventory rather
                // than manufactured. Accumulate rather than overwrite: the same product can be
                // a leaf requirement in multiple branches of the same explosion.
                Accumulate(leafTotals, line.ComponentProductId, requiredQuantity);
                continue;
            }

            if (pathBomIds.Contains(childBom.Id))
            {
                // Circular BOM reference: this component's own BOM is an ancestor of the node
                // we're currently exploding (e.g. product A's BOM contains product B, whose
                // primary BOM in turn contains product A). Recursing would never terminate, so
                // stop here, report it, and fall back to treating this occurrence as a leaf
                // requirement rather than silently producing a wrong (or hung) result.
                warnings.Add(
                    $"Circular bill of materials reference detected: bill of materials '{childBom.Id}' " +
                    $"(component product '{line.ComponentProductId}') already appears earlier in this " +
                    "explosion's own recursion path. It was treated as a leaf requirement instead of being " +
                    "exploded again to avoid infinite recursion.");
                Accumulate(leafTotals, line.ComponentProductId, requiredQuantity);
                continue;
            }

            await ExplodeRecursiveAsync(repository, companyId, childBom, requiredQuantity, pathBomIds, leafTotals, warnings, depth + 1, cancellationToken);
        }

        pathBomIds.Remove(bom.Id);
    }

    private static void Accumulate(Dictionary<Guid, decimal> totals, Guid productId, decimal quantity) =>
        totals[productId] = totals.TryGetValue(productId, out var existing) ? existing + quantity : quantity;
}
