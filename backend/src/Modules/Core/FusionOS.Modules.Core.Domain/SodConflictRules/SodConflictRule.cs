using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.SodConflictRules;

/// <summary>
/// A lightweight Segregation-of-Duties (SoD) conflict rule (Phase 2.2 roadmap) — NOT a full
/// GRC/access-certification engine, just a practical guard against one person both creating
/// AND approving the same high-risk transaction. A rule names a pair of permission codes
/// (e.g. "procurement.purchase-order.create" and "procurement.purchase-order.approve") that
/// must never both be exercised by the same user against the same record. Enforcement itself
/// lives at the specific approval-decision call site (see
/// FusionOS.BuildingBlocks.Application.Abstractions.ISegregationOfDutiesService and its
/// callers) — this entity is only the configurable catalog of which permission pairs count
/// as a conflict, so new conflicts can be declared without a code change.
///
/// Modeled per-company (TenantAggregateRoot), same convention as every other master-data
/// entity in this codebase (e.g. WithholdingTaxRate). ConflictingPermissionA/B intentionally
/// store raw permission-code strings rather than a foreign key into the Permission table —
/// PermissionCatalog is the single source of truth for valid codes and is a static in-code
/// catalog, not a seeded-then-queried table other modules can join against across the
/// module-boundary rule (03_SYSTEM_ARCHITECTURE.md §2).
/// </summary>
public sealed class SodConflictRule : TenantAggregateRoot
{
    public string ConflictingPermissionA { get; private set; } = default!;
    public string ConflictingPermissionB { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private SodConflictRule() { }

    public static SodConflictRule Create(Guid companyId, string conflictingPermissionA, string conflictingPermissionB, string? description)
    {
        if (string.IsNullOrWhiteSpace(conflictingPermissionA))
            throw new ArgumentException("The first conflicting permission code is required.", nameof(conflictingPermissionA));
        if (string.IsNullOrWhiteSpace(conflictingPermissionB))
            throw new ArgumentException("The second conflicting permission code is required.", nameof(conflictingPermissionB));

        var permissionA = conflictingPermissionA.Trim();
        var permissionB = conflictingPermissionB.Trim();

        if (string.Equals(permissionA, permissionB, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("A conflict rule must name two different permission codes.", nameof(conflictingPermissionB));

        return new SodConflictRule
        {
            CompanyId = companyId,
            ConflictingPermissionA = permissionA,
            ConflictingPermissionB = permissionB,
            Description = NormalizeDescription(description),
            IsActive = true,
        };
    }

    /// <summary>Soft on/off switch, never a hard delete — same convention as WithholdingTaxRate.Deactivate().</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// True when this active rule covers the given permission pair, in either order —
    /// a rule for (A, B) also conflicts for a caller checking (B, A).
    /// </summary>
    public bool Covers(string permissionA, string permissionB) =>
        IsActive &&
        ((string.Equals(ConflictingPermissionA, permissionA, StringComparison.OrdinalIgnoreCase) && string.Equals(ConflictingPermissionB, permissionB, StringComparison.OrdinalIgnoreCase)) ||
         (string.Equals(ConflictingPermissionA, permissionB, StringComparison.OrdinalIgnoreCase) && string.Equals(ConflictingPermissionB, permissionA, StringComparison.OrdinalIgnoreCase)));

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
