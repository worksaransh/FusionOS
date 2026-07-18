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
}
