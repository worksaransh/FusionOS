using FusionOS.SharedKernel;
using FusionOS.Modules.Warehouse.Domain.Bins.Events;

namespace FusionOS.Modules.Warehouse.Domain.Bins;

/// <summary>
/// A Bin is the finest-grained storage location this codebase tracks —
/// nests under Zone exactly the way Zone nests under Warehouse (ZoneId is an
/// in-module reference to Zone, a real FK). This is the "Zones get a Bin
/// sub-entity" item named in docs/IMPLEMENTATION_PLAN.md Phase 9 / the
/// "bins" item in docs/PROJECT_TRACKER.md's Phase M9 WMS-depth scope.
///
/// Deliberately mirrors Zone's shape exactly (Code/Name/IsActive, same
/// Create/UpdateDetails/Deactivate lifecycle) rather than inventing a
/// different pattern for one more level of nesting.
/// </summary>
public sealed class Bin : TenantAggregateRoot
{
    public Guid ZoneId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Optional refinement of this bin's location, one level deeper than the
    /// required ZoneId (Warehouse -> Zone -> Rack -> Shelf -> Bin, with only
    /// Warehouse/Zone required). Nullable and additive — every existing
    /// Bin-creation flow that only specifies a Zone keeps working unchanged.
    /// Assigned/cleared via AssignShelf; AssignBinShelfCommandHandler is
    /// responsible for verifying the Shelf's Rack's Zone matches this bin's
    /// own ZoneId before calling it (data-integrity rule lives in the
    /// application layer, same place CreateBinCommandHandler checks
    /// ZoneExistsAsync, since it needs a cross-aggregate repository lookup
    /// the domain method itself has no access to).
    /// </summary>
    public Guid? ShelfId { get; private set; }

    private Bin() { }

    public static Bin Create(Guid companyId, Guid zoneId, string name, string code)
    {
        if (zoneId == Guid.Empty)
            throw new ArgumentException("Zone id is required.", nameof(zoneId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bin name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Bin code is required.", nameof(code));

        var bin = new Bin
        {
            CompanyId = companyId,
            ZoneId = zoneId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
        };

        bin.Raise(new BinCreated(bin.Id, companyId, zoneId, bin.Code));
        return bin;
    }

    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Covers Name only — ZoneId is the bin's parent FK and Code is the
    /// business key (uniqueness-checked at creation, scoped to
    /// company+zone), same immutability rule as Zone.UpdateDetails.
    /// </summary>
    public void UpdateDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bin name is required.", nameof(name));

        Name = name.Trim();
    }

    /// <summary>
    /// Sets or clears (pass null) this bin's optional Shelf refinement.
    /// Zone-consistency (the Shelf's Rack's Zone must match this bin's
    /// ZoneId) is enforced by AssignBinShelfCommandHandler before this is
    /// called — this method trusts its caller, same as Create trusting
    /// CreateBinCommandHandler's prior ZoneExistsAsync check.
    /// </summary>
    public void AssignShelf(Guid? shelfId) => ShelfId = shelfId;
}
