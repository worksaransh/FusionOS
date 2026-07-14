using FusionOS.SharedKernel;
using FusionOS.Modules.Warehouse.Domain.Zones.Events;

namespace FusionOS.Modules.Warehouse.Domain.Zones;

/// <summary>
/// A Zone belongs to exactly one Warehouse (WarehouseId is an in-module
/// reference to Warehouse.Warehouse — same module, so this IS a real FK,
/// unlike Inventory's ledger references into other modules). Rack/Shelf/Bin
/// nest under Zone in a later slice; receiving/put-away/picking/packing
/// reference Zones once they exist.
/// </summary>
public sealed class Zone : TenantAggregateRoot
{
    public Guid WarehouseId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Zone() { }

    public static Zone Create(Guid companyId, Guid warehouseId, string name, string code)
    {
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Zone name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Zone code is required.", nameof(code));

        var zone = new Zone
        {
            CompanyId = companyId,
            WarehouseId = warehouseId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
        };

        zone.Raise(new ZoneCreated(zone.Id, companyId, warehouseId, zone.Code));
        return zone;
    }

    public void Deactivate() => IsActive = false;
}
