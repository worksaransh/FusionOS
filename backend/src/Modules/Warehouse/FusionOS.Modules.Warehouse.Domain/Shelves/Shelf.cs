using FusionOS.SharedKernel;
using FusionOS.Modules.Warehouse.Domain.Shelves.Events;

namespace FusionOS.Modules.Warehouse.Domain.Shelves;

/// <summary>
/// A Shelf is an optional storage level nested under Rack — nests under Rack
/// exactly the way Rack nests under Zone (RackId is an in-module reference
/// to Rack, a real FK). Bin can optionally reference a Shelf (Bin.ShelfId)
/// for more precise location, on top of Bin's existing required ZoneId.
///
/// Deliberately mirrors Zone/Bin/Rack's shape exactly (Code/Name/IsActive,
/// same Create/UpdateDetails/Deactivate lifecycle) rather than inventing a
/// different pattern for one more level of nesting.
/// </summary>
public sealed class Shelf : TenantAggregateRoot
{
    public Guid RackId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Shelf() { }

    public static Shelf Create(Guid companyId, Guid rackId, string name, string code)
    {
        if (rackId == Guid.Empty)
            throw new ArgumentException("Rack id is required.", nameof(rackId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Shelf name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Shelf code is required.", nameof(code));

        var shelf = new Shelf
        {
            CompanyId = companyId,
            RackId = rackId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
        };

        shelf.Raise(new ShelfCreated(shelf.Id, companyId, rackId, shelf.Code));
        return shelf;
    }

    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Covers Name only — RackId is the shelf's parent FK and Code is the
    /// business key (uniqueness-checked at creation, scoped to
    /// company+rack), same immutability rule as Rack.UpdateDetails.
    /// </summary>
    public void UpdateDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Shelf name is required.", nameof(name));

        Name = name.Trim();
    }
}
