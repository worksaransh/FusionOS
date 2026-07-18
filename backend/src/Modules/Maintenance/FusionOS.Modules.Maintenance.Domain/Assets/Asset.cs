using FusionOS.SharedKernel;
using FusionOS.Modules.Maintenance.Domain.Assets.Events;

namespace FusionOS.Modules.Maintenance.Domain.Assets;

/// <summary>
/// Phase 5 — Maintenance, first slice: the machine register
/// (05_MODULE_ROADMAP.md's "Machine register" line item). Pure master data
/// (Code/Name/Location/IsActive), same shape as Finance's CostCenter — no
/// hierarchy, no meter-reading/warranty tracking yet; those are separately
/// scoped follow-ups once a MaintenanceRequest exists to reference this.
/// Location is a plain optional string, not a cross-module WarehouseId
/// reference — a machine's physical location description ("Line 2, Bay 4")
/// doesn't need to resolve to a real Warehouse aggregate for this slice to
/// be useful, and adding that reference now would be scope not asked for.
/// </summary>
public sealed class Asset : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Location { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Asset() { }

    public static Asset Create(Guid companyId, string code, string name, string? location)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Asset code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Asset name is required.", nameof(name));

        var asset = new Asset
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
        };

        asset.Raise(new AssetCreated(asset.Id, companyId, asset.Code));
        return asset;
    }

    public void Deactivate() => IsActive = false;
}
