using FusionOS.SharedKernel;
using FusionOS.Modules.Warehouse.Domain.Racks.Events;

namespace FusionOS.Modules.Warehouse.Domain.Racks;

/// <summary>
/// A Rack is an optional intermediate storage level between Zone and Bin —
/// nests under Zone exactly the way Bin nests under Zone (ZoneId is an
/// in-module reference to Zone, a real FK). Added alongside Shelf to give
/// Bin a more precise, OPTIONAL location beyond its required Zone
/// (Warehouse -> Zone -> Rack -> Shelf -> Bin, with only Warehouse/Zone
/// required and Rack/Shelf/Bin.ShelfId optional refinements).
///
/// Deliberately mirrors Zone/Bin's shape exactly (Code/Name/IsActive, same
/// Create/UpdateDetails/Deactivate lifecycle) rather than inventing a
/// different pattern for one more level of nesting.
/// </summary>
public sealed class Rack : TenantAggregateRoot
{
    public Guid ZoneId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Rack() { }

    public static Rack Create(Guid companyId, Guid zoneId, string name, string code)
    {
        if (zoneId == Guid.Empty)
            throw new ArgumentException("Zone id is required.", nameof(zoneId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rack name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Rack code is required.", nameof(code));

        var rack = new Rack
        {
            CompanyId = companyId,
            ZoneId = zoneId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
        };

        rack.Raise(new RackCreated(rack.Id, companyId, zoneId, rack.Code));
        return rack;
    }

    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Covers Name only — ZoneId is the rack's parent FK and Code is the
    /// business key (uniqueness-checked at creation, scoped to
    /// company+zone), same immutability rule as Zone.UpdateDetails/Bin.UpdateDetails.
    /// </summary>
    public void UpdateDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rack name is required.", nameof(name));

        Name = name.Trim();
    }
}
