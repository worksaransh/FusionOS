using FusionOS.SharedKernel;
using FusionOS.Modules.Warehouse.Domain.Warehouses.Events;

namespace FusionOS.Modules.Warehouse.Domain.Warehouses;

/// <summary>
/// The anchor aggregate for Warehouse Management (05_MODULE_ROADMAP.md Phase 1).
/// Zones/Rack/Shelf/Bin, receiving, put-away, picking, packing, and dispatch all
/// build on top of a Warehouse in later slices — this is the narrow first cut:
/// the physical location itself.
/// </summary>
public sealed class Warehouse : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string? Address { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Warehouse() { }

    public static Warehouse Create(Guid companyId, Guid? branchId, string name, string code, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Warehouse code is required.", nameof(code));

        var warehouse = new Warehouse
        {
            CompanyId = companyId,
            BranchId = branchId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Address = address,
        };

        warehouse.Raise(new WarehouseCreated(warehouse.Id, companyId, warehouse.Code));
        return warehouse;
    }

    public void Deactivate() => IsActive = false;
}
